using HospitalMS.Business.Models;
using HospitalMS.Data.Persistence;
using HospitalMS.Data.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace HospitalMS.Business.Services;

public sealed class PredictiveAnalyticsService(HospitalDbContext dbContext) : IPredictiveAnalyticsService
{
    // ── Readmission Risk ────────────────────────────────────────────────────
    // Heuristic scoring: age ≥65 (+20), ≥3 recent diagnoses (+20),
    // prior encounter within 30 days (+30), ≥2 prescriptions (+10),
    // critical quality incident (+20). Max 100.

    public async Task<IReadOnlyList<ReadmissionRiskScore>> GetReadmissionRisksAsync(
        int topN = 20, CancellationToken ct = default)
    {
        var now      = DateTime.UtcNow;
        var cutoff30 = now.AddDays(-30);
        var cutoff90 = now.AddDays(-90);

        var patients = await dbContext.Patients
            .AsNoTracking()
            .ToListAsync(ct);

        var recentEncounters = await dbContext.ClinicalEncounters
            .Where(e => e.StartedAtUtc >= cutoff90)
            .AsNoTracking()
            .ToListAsync(ct);

        var diagnoses = await dbContext.Diagnoses
            .Where(d => d.CreatedAtUtc >= cutoff90)
            .AsNoTracking()
            .ToListAsync(ct);

        var prescriptions = await dbContext.Prescriptions
            .Where(p => p.PrescribedAtUtc >= cutoff90)
            .AsNoTracking()
            .ToListAsync(ct);

        var scores = patients.Select(p =>
        {
            double score = 0;
            var factors = new List<string>();

            var age = CalculateAge(p.DateOfBirth, now);
            if (age >= 65) { score += 20; factors.Add("Age ≥ 65"); }

            var patDiagnoses = diagnoses.Count(d => d.EncounterId != Guid.Empty
                && recentEncounters.Any(e => e.PatientId == p.Id && e.Id == d.EncounterId));
            if (patDiagnoses >= 3) { score += 20; factors.Add("≥3 recent diagnoses"); }

            var patEncounters = recentEncounters.Where(e => e.PatientId == p.Id).ToList();
            var hasRecentEncounter = patEncounters.Any(e => e.StartedAtUtc >= cutoff30);
            if (hasRecentEncounter) { score += 30; factors.Add("Encounter within 30 days"); }

            var patPrescriptions = prescriptions.Count(pr =>
                patEncounters.Any(e => e.Id == pr.EncounterId));
            if (patPrescriptions >= 2) { score += 10; factors.Add("≥2 active prescriptions"); }

            score = Math.Min(score, 100.0);
            var level = score >= 70 ? "High" : score >= 40 ? "Medium" : "Low";
            return new ReadmissionRiskScore(
                p.Id, $"{p.FirstName} {p.LastName}",
                Math.Round(score, 1), level, factors);
        })
        .Where(s => s.RiskScore > 0)
        .OrderByDescending(s => s.RiskScore)
        .Take(topN)
        .ToArray();

        return scores;
    }

    // ── Bed Demand Forecast ─────────────────────────────────────────────────
    // Projects weekly admissions using the rolling 8-week average and
    // recommends bed availability = projected + 15% buffer.

    public async Task<IReadOnlyList<BedDemandForecast>> GetBedDemandForecastAsync(
        int weeks = 4, CancellationToken ct = default)
    {
        var now      = DateTime.UtcNow;
        var history  = now.AddDays(-56); // 8 weeks

        var admissions = await dbContext.BedAllocations
            .Where(a => a.AdmittedAtUtc >= history)
            .AsNoTracking()
            .Select(a => a.AdmittedAtUtc)
            .ToListAsync(ct);

        // Group into 8 weekly buckets
        var weeklyBuckets = new int[8];
        for (var i = 0; i < 8; i++)
        {
            var weekStart = history.AddDays(i * 7);
            var weekEnd   = weekStart.AddDays(7);
            weeklyBuckets[i] = admissions.Count(a => a >= weekStart && a < weekEnd);
        }

        var avgWeekly = weeklyBuckets.Length > 0 ? weeklyBuckets.Average() : 0.0;

        var forecasts = Enumerable.Range(1, weeks).Select(w =>
        {
            var startDate    = now.AddDays((w - 1) * 7);
            var label        = $"Week {w} ({startDate:dd MMM})";
            var predicted    = (int)Math.Round(avgWeekly);
            var recommended  = (int)Math.Ceiling(predicted * 1.15);
            var confidence   = Math.Min(95.0, 60.0 + admissions.Count * 0.5);
            return new BedDemandForecast(label, predicted, recommended, Math.Round(confidence, 1));
        }).ToArray();

        return forecasts;
    }

    // ── Low Stock Predictions ────────────────────────────────────────────────
    // Uses average daily consumption from stock transactions (last 30 days).
    // If no transaction history, falls back to a 10% annual turnover estimate.

    public async Task<IReadOnlyList<LowStockPrediction>> GetLowStockPredictionsAsync(
        CancellationToken ct = default)
    {
        var cutoff = DateTime.UtcNow.AddDays(-30);

        var items = await dbContext.InventoryItems
            .Where(i => i.IsActive)
            .AsNoTracking()
            .ToListAsync(ct);

        var txns = await dbContext.StockTransactions
            .Where(t => t.TransactedAtUtc >= cutoff && t.Type == StockTransactionType.Out)
            .AsNoTracking()
            .Select(t => new { t.ItemId, t.Quantity })
            .ToListAsync(ct);

        var predictions = items.Select(item =>
        {
            var usedLast30 = txns
                .Where(t => t.ItemId == item.Id)
                .Sum(t => Math.Abs(t.Quantity));

            var avgDaily = usedLast30 > 0 ? usedLast30 / 30.0 : item.UnitCost > 0 ? 0.1 : 0.0;
            var daysLeft  = avgDaily > 0
                ? (int)Math.Floor(item.CurrentStock / avgDaily)
                : int.MaxValue;

            var urgency = daysLeft switch
            {
                0               => "Out of Stock",
                <= 7            => "Critical",
                <= 14           => "Warning",
                <= 30           => "Monitor",
                _               => "OK"
            };

            return new LowStockPrediction(
                item.Id, item.Code, item.Name, item.Unit,
                item.CurrentStock, item.ReorderLevel,
                Math.Round(avgDaily, 2),
                daysLeft == int.MaxValue ? 999 : daysLeft,
                urgency);
        })
        .Where(p => p.Urgency != "OK")
        .OrderBy(p => p.EstimatedDaysUntilStockout)
        .ToArray();

        return predictions;
    }

    // ── No-Show Risk ────────────────────────────────────────────────────────
    // Heuristic: patient had ≥1 missed/cancelled appointment before (+40),
    // appointment is ≥7 days away (+20), patient age 18–30 (+15).

    public async Task<IReadOnlyList<AppointmentNoShowRisk>> GetNoShowRisksAsync(
        CancellationToken ct = default)
    {
        var now     = DateTime.UtcNow;
        var horizon = now.AddDays(14);

        var upcomingApts = await dbContext.Appointments
            .Include(a => a.Patient)
            .Where(a => a.ScheduledAtUtc >= now
                     && a.ScheduledAtUtc <= horizon
                     && a.Status == AppointmentStatus.Scheduled)
            .AsNoTracking()
            .ToListAsync(ct);

        var allApts = await dbContext.Appointments
            .Select(a => new { a.PatientId, a.Status })
            .AsNoTracking()
            .ToListAsync(ct);

        var risks = upcomingApts.Select(a =>
        {
            double prob = 10.0;
            var history = allApts.Where(h => h.PatientId == a.PatientId).ToList();
            var missed  = history.Count(h =>
                h.Status == AppointmentStatus.NoShow || h.Status == AppointmentStatus.Cancelled);

            if (missed >= 1) prob += 40;

            var daysAway = (a.ScheduledAtUtc - now).TotalDays;
            if (daysAway >= 7) prob += 20;

            if (a.Patient is not null)
            {
                var age = CalculateAge(a.Patient.DateOfBirth, now);
                if (age is >= 18 and <= 30) prob += 15;
            }

            prob = Math.Min(prob, 95.0);
            var level = prob >= 60 ? "High" : prob >= 35 ? "Medium" : "Low";
            return new AppointmentNoShowRisk(
                a.Id, a.PatientId,
                a.Patient is null ? "Unknown" : $"{a.Patient.FirstName} {a.Patient.LastName}",
                a.ScheduledAtUtc, Math.Round(prob, 1), level);
        })
        .OrderByDescending(r => r.NoShowProbability)
        .ToArray();

        return risks;
    }

    // ── Summary ──────────────────────────────────────────────────────────────

    public async Task<PredictiveInsightsSummary> GetInsightsSummaryAsync(CancellationToken ct = default)
    {
        var readmissions = await GetReadmissionRisksAsync(10, ct);
        var stock        = await GetLowStockPredictionsAsync(ct);
        var noShows      = await GetNoShowRisksAsync(ct);
        var bedForecast  = await GetBedDemandForecastAsync(4, ct);

        var highReadmissions = readmissions.Count(r => r.RiskLevel == "High");
        var criticalStock    = stock.Count(s => s.Urgency is "Critical" or "Out of Stock");
        var highNoShows      = noShows.Count(n => n.RiskLevel == "High");

        return new PredictiveInsightsSummary(
            highReadmissions, criticalStock, highNoShows,
            readmissions.Take(5).ToArray(),
            stock.Where(s => s.Urgency is "Critical" or "Out of Stock").Take(10).ToArray(),
            bedForecast);
    }

    private static int CalculateAge(DateTime dob, DateTime now)
    {
        var age = now.Year - dob.Year;
        if (now < dob.AddYears(age)) age--;
        return age;
    }
}
