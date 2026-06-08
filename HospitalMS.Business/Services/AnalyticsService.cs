using HospitalMS.Business.Models;
using HospitalMS.Data.Persistence;
using HospitalMS.Data.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace HospitalMS.Business.Services;

public sealed class AnalyticsService(HospitalDbContext dbContext) : IAnalyticsService
{
    public async Task<DashboardSummary> GetDashboardSummaryAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var todayStart = now.Date;
        var weekStart = todayStart.AddDays(-(int)now.DayOfWeek);
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var totalPatients        = await dbContext.Patients.CountAsync(cancellationToken);
        var newPatientsThisMonth = await dbContext.Patients
            .CountAsync(p => p.CreatedAtUtc >= monthStart, cancellationToken);

        var appointmentsToday = await dbContext.Appointments
            .CountAsync(a => a.ScheduledAtUtc >= todayStart && a.ScheduledAtUtc < todayStart.AddDays(1), cancellationToken);
        var appointmentsThisWeek = await dbContext.Appointments
            .CountAsync(a => a.ScheduledAtUtc >= weekStart, cancellationToken);

        var totalBeds    = await dbContext.Beds.CountAsync(b => b.IsActive, cancellationToken);
        var occupiedBeds = await dbContext.BedAllocations.CountAsync(a => a.DischargedAtUtc == null, cancellationToken);
        var occupancyRate = totalBeds > 0 ? Math.Round((decimal)occupiedBeds / totalBeds * 100, 1) : 0m;

        var openMaintenance = await dbContext.MaintenanceRequests
            .CountAsync(m => m.Status == MaintenanceRequestStatus.Open
                          || m.Status == MaintenanceRequestStatus.InProgress, cancellationToken);

        var invoices = await dbContext.Invoices
            .Where(i => i.Status != InvoiceStatus.Paid && i.Status != InvoiceStatus.Voided)
            .Select(i => new { Balance = i.TotalAmount - i.PaidAmount })
            .ToListAsync(cancellationToken);
        var outstanding = invoices.Sum(i => i.Balance > 0 ? i.Balance : 0);

        var lowStock = await dbContext.InventoryItems
            .CountAsync(i => i.IsActive && i.CurrentStock <= i.ReorderLevel, cancellationToken);

        return new DashboardSummary(
            totalPatients, newPatientsThisMonth,
            appointmentsToday, appointmentsThisWeek,
            occupiedBeds, totalBeds, occupancyRate,
            openMaintenance, outstanding, lowStock);
    }

    public async Task<PatientDemographicsReport> GetPatientDemographicsAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var patients = await dbContext.Patients
            .Select(p => new { p.Gender, p.DateOfBirth, p.CreatedAtUtc })
            .ToListAsync(cancellationToken);

        var total = patients.Count;

        var byGender = patients
            .GroupBy(p => string.IsNullOrWhiteSpace(p.Gender) ? "Unknown" : p.Gender)
            .Select(g => new LabelCount(g.Key, g.Count()))
            .OrderByDescending(x => x.Count)
            .ToArray();

        var byAge = patients
            .Select(p => CalculateAge(p.DateOfBirth, now))
            .GroupBy(age => age switch
            {
                < 18  => "0–17",
                < 35  => "18–34",
                < 55  => "35–54",
                < 75  => "55–74",
                _     => "75+"
            })
            .Select(g => new LabelCount(g.Key, g.Count()))
            .OrderBy(x => x.Label)
            .ToArray();

        var monthStart12 = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-11);
        var newPerMonth = patients
            .Where(p => p.CreatedAtUtc >= monthStart12)
            .GroupBy(p => new { p.CreatedAtUtc.Year, p.CreatedAtUtc.Month })
            .Select(g => new MonthlyCount(g.Key.Year, g.Key.Month,
                $"{g.Key.Year}-{g.Key.Month:00}", g.Count()))
            .OrderBy(x => x.Year).ThenBy(x => x.Month)
            .ToArray();

        return new PatientDemographicsReport(total, byGender, byAge, newPerMonth);
    }

    public async Task<AppointmentReport> GetAppointmentReportAsync(
        DateTime? from = null, DateTime? to = null, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Appointments.AsQueryable();
        if (from.HasValue) query = query.Where(a => a.ScheduledAtUtc >= from.Value);
        if (to.HasValue)   query = query.Where(a => a.ScheduledAtUtc <= to.Value);

        var appointments = await query
            .Select(a => new { a.Status, a.ScheduledAtUtc })
            .ToListAsync(cancellationToken);

        var byStatus = appointments
            .GroupBy(a => a.Status)
            .Select(g => new LabelCount(g.Key, g.Count()))
            .OrderByDescending(x => x.Count)
            .ToArray();

        var cutoff = DateTime.UtcNow.AddMonths(-11);
        var cutoffDate = new DateTime(cutoff.Year, cutoff.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var perMonth = appointments
            .Where(a => a.ScheduledAtUtc >= cutoffDate)
            .GroupBy(a => new { a.ScheduledAtUtc.Year, a.ScheduledAtUtc.Month })
            .Select(g => new MonthlyCount(g.Key.Year, g.Key.Month,
                $"{g.Key.Year}-{g.Key.Month:00}", g.Count()))
            .OrderBy(x => x.Year).ThenBy(x => x.Month)
            .ToArray();

        return new AppointmentReport(appointments.Count, byStatus, perMonth);
    }

    public async Task<RevenueReport> GetRevenueReportAsync(CancellationToken cancellationToken = default)
    {
        var payments = await dbContext.Payments
            .Select(p => new { p.Amount, p.Method, p.PaidAtUtc })
            .ToListAsync(cancellationToken);

        var invoices = await dbContext.Invoices
            .Where(i => i.Status != InvoiceStatus.Paid && i.Status != InvoiceStatus.Voided)
            .Select(i => new { Balance = i.TotalAmount - i.PaidAmount })
            .ToListAsync(cancellationToken);

        var totalRevenue = payments.Sum(p => p.Amount);
        var outstanding  = invoices.Sum(i => i.Balance > 0 ? i.Balance : 0);

        var cutoff = DateTime.UtcNow.AddMonths(-11);
        var cutoffDate = new DateTime(cutoff.Year, cutoff.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var perMonth = payments
            .Where(p => p.PaidAtUtc >= cutoffDate)
            .GroupBy(p => new { p.PaidAtUtc.Year, p.PaidAtUtc.Month })
            .Select(g => new MonthlyRevenue(g.Key.Year, g.Key.Month,
                $"{g.Key.Year}-{g.Key.Month:00}", g.Sum(p => p.Amount)))
            .OrderBy(x => x.Year).ThenBy(x => x.Month)
            .ToArray();

        var byMethod = payments
            .GroupBy(p => p.Method)
            .Select(g => new LabelAmount(g.Key, g.Sum(p => p.Amount)))
            .OrderByDescending(x => x.Amount)
            .ToArray();

        return new RevenueReport(totalRevenue, outstanding, perMonth, byMethod);
    }

    public async Task<BedOccupancyReport> GetBedOccupancyReportAsync(CancellationToken cancellationToken = default)
    {
        var wards = await dbContext.Wards
            .Include(w => w.Beds).ThenInclude(b => b.Allocations)
            .Where(w => w.IsActive)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var rows = wards.Select(w =>
        {
            var activeBeds   = w.Beds.Where(b => b.IsActive).ToList();
            var occupied     = activeBeds.Count(b => b.Allocations.Any(a => a.DischargedAtUtc == null));
            var total        = activeBeds.Count;
            var rate         = total > 0 ? Math.Round((decimal)occupied / total * 100, 1) : 0m;
            return new WardOccupancyRow(w.Id, w.Name, total, occupied, rate);
        }).ToArray();

        var totalBeds    = rows.Sum(r => r.TotalBeds);
        var occupiedBeds = rows.Sum(r => r.OccupiedBeds);
        var available    = totalBeds - occupiedBeds;
        var rate         = totalBeds > 0 ? Math.Round((decimal)occupiedBeds / totalBeds * 100, 1) : 0m;

        return new BedOccupancyReport(totalBeds, occupiedBeds, available, rate, rows);
    }

    public async Task<InventoryStatusReport> GetInventoryStatusReportAsync(CancellationToken cancellationToken = default)
    {
        var items = await dbContext.InventoryItems
            .Where(i => i.IsActive)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var total    = items.Count;
        var lowItems = items.Where(i => i.CurrentStock <= i.ReorderLevel).ToList();

        var lowSummaries = lowItems
            .OrderBy(i => i.CurrentStock - i.ReorderLevel)
            .Select(i => new LowStockItemSummary(i.Id, i.Code, i.Name, i.Unit, i.CurrentStock, i.ReorderLevel))
            .ToArray();

        return new InventoryStatusReport(total, lowItems.Count, lowSummaries);
    }

    private static int CalculateAge(DateTime dob, DateTime now)
    {
        var age = now.Year - dob.Year;
        if (now < dob.AddYears(age)) age--;
        return age;
    }
}
