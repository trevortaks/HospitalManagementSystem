using HospitalMS.Business.Models;
using HospitalMS.Data.Persistence;
using HospitalMS.Data.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace HospitalMS.Business.Services;

public sealed class QualityService(HospitalDbContext dbContext) : IQualityService
{
    public async Task<QualityIncidentResponse> CreateIncidentAsync(
        CreateQualityIncidentRequest request, CancellationToken ct = default)
    {
        var incident = new QualityIncident
        {
            Title        = request.Title,
            Description  = request.Description,
            IncidentType = request.IncidentType,
            Severity     = request.Severity,
            Status       = QualityIncidentStatus.Open,
            ReportedByUserId = request.ReportedByUserId,
            PatientId    = request.PatientId,
            Location     = request.Location,
            OccurredAtUtc  = request.OccurredAtUtc?.ToUniversalTime() ?? DateTime.UtcNow,
            ReportedAtUtc  = DateTime.UtcNow
        };

        dbContext.QualityIncidents.Add(incident);
        await dbContext.SaveChangesAsync(ct);
        return await GetIncidentByIdAsync(incident.Id, ct);
    }

    public async Task<IReadOnlyList<QualityIncidentResponse>> GetIncidentsAsync(
        string? status = null, string? severity = null, CancellationToken ct = default)
    {
        var query = dbContext.QualityIncidents
            .Include(q => q.ReportedByUser)
            .Include(q => q.AssignedToUser)
            .Include(q => q.Patient)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrEmpty(status))
            query = query.Where(q => q.Status == status);
        if (!string.IsNullOrEmpty(severity))
            query = query.Where(q => q.Severity == severity);

        var list = await query.OrderByDescending(q => q.OccurredAtUtc).ToListAsync(ct);
        return list.Select(ToResponse).ToArray();
    }

    public async Task<QualityIncidentResponse> GetIncidentByIdAsync(Guid id, CancellationToken ct = default)
    {
        var incident = await dbContext.QualityIncidents
            .Include(q => q.ReportedByUser)
            .Include(q => q.AssignedToUser)
            .Include(q => q.Patient)
            .AsNoTracking()
            .FirstOrDefaultAsync(q => q.Id == id, ct)
            ?? throw new KeyNotFoundException($"Incident {id} not found.");
        return ToResponse(incident);
    }

    public async Task<QualityIncidentResponse> AssignIncidentAsync(
        Guid id, AssignQualityIncidentRequest request, CancellationToken ct = default)
    {
        var incident = await dbContext.QualityIncidents.FindAsync([id], ct)
            ?? throw new KeyNotFoundException($"Incident {id} not found.");

        incident.AssignedToUserId = request.AssignedToUserId;
        if (incident.Status == QualityIncidentStatus.Open)
            incident.Status = QualityIncidentStatus.UnderInvestigation;

        await dbContext.SaveChangesAsync(ct);
        return await GetIncidentByIdAsync(id, ct);
    }

    public async Task<QualityIncidentResponse> StartInvestigationAsync(
        Guid id, InvestigateQualityIncidentRequest request, CancellationToken ct = default)
    {
        var incident = await dbContext.QualityIncidents.FindAsync([id], ct)
            ?? throw new KeyNotFoundException($"Incident {id} not found.");

        incident.Status          = QualityIncidentStatus.UnderInvestigation;
        incident.RootCause       = request.RootCause;
        incident.CorrectiveAction = request.CorrectiveAction;

        await dbContext.SaveChangesAsync(ct);
        return await GetIncidentByIdAsync(id, ct);
    }

    public async Task<QualityIncidentResponse> ResolveIncidentAsync(
        Guid id, ResolveQualityIncidentRequest request, CancellationToken ct = default)
    {
        var incident = await dbContext.QualityIncidents.FindAsync([id], ct)
            ?? throw new KeyNotFoundException($"Incident {id} not found.");

        if (incident.Status == QualityIncidentStatus.Resolved
         || incident.Status == QualityIncidentStatus.Closed)
            throw new InvalidOperationException($"Incident is already {incident.Status}.");

        incident.Status        = QualityIncidentStatus.Resolved;
        incident.ResolvedAtUtc = DateTime.UtcNow;
        if (!string.IsNullOrEmpty(request.ResolutionNotes))
            incident.CorrectiveAction = request.ResolutionNotes;

        await dbContext.SaveChangesAsync(ct);
        return await GetIncidentByIdAsync(id, ct);
    }

    public async Task<QualityIncidentResponse> CloseIncidentAsync(Guid id, CancellationToken ct = default)
    {
        var incident = await dbContext.QualityIncidents.FindAsync([id], ct)
            ?? throw new KeyNotFoundException($"Incident {id} not found.");

        if (incident.Status == QualityIncidentStatus.Closed)
            throw new InvalidOperationException("Incident is already Closed.");

        incident.Status = QualityIncidentStatus.Closed;
        incident.ResolvedAtUtc ??= DateTime.UtcNow;

        await dbContext.SaveChangesAsync(ct);
        return await GetIncidentByIdAsync(id, ct);
    }

    public async Task<PatientFeedbackResponse> SubmitFeedbackAsync(
        CreatePatientFeedbackRequest request, CancellationToken ct = default)
    {
        if (request.OverallRating is < 1 or > 5)
            throw new ArgumentException("OverallRating must be between 1 and 5.");

        var feedback = new PatientFeedback
        {
            PatientId      = request.PatientId,
            AppointmentId  = request.AppointmentId,
            OverallRating  = request.OverallRating,
            StaffRating    = request.StaffRating,
            FacilityRating = request.FacilityRating,
            Comments       = request.Comments,
            SubmittedAtUtc = DateTime.UtcNow
        };

        dbContext.PatientFeedback.Add(feedback);
        await dbContext.SaveChangesAsync(ct);

        var patient = await dbContext.Patients.FindAsync([request.PatientId], ct);
        return ToFeedbackResponse(feedback, patient);
    }

    public async Task<IReadOnlyList<PatientFeedbackResponse>> GetFeedbackAsync(
        Guid? patientId = null, CancellationToken ct = default)
    {
        var query = dbContext.PatientFeedback
            .Include(f => f.Patient)
            .AsNoTracking()
            .AsQueryable();

        if (patientId.HasValue)
            query = query.Where(f => f.PatientId == patientId.Value);

        var list = await query.OrderByDescending(f => f.SubmittedAtUtc).ToListAsync(ct);
        return list.Select(f => ToFeedbackResponse(f, f.Patient)).ToArray();
    }

    public async Task<QualitySummary> GetSummaryAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var incidents = await dbContext.QualityIncidents.AsNoTracking().ToListAsync(ct);
        var feedback  = await dbContext.PatientFeedback.AsNoTracking().ToListAsync(ct);

        var open             = incidents.Count(i => i.Status == QualityIncidentStatus.Open);
        var underInv         = incidents.Count(i => i.Status == QualityIncidentStatus.UnderInvestigation);
        var resolvedThisMonth = incidents.Count(i =>
            (i.Status == QualityIncidentStatus.Resolved || i.Status == QualityIncidentStatus.Closed)
            && i.ResolvedAtUtc >= monthStart);

        var avgRating = feedback.Count > 0 ? feedback.Average(f => (double)f.OverallRating) : 0.0;

        var byType = incidents
            .GroupBy(i => i.IncidentType)
            .Select(g => new LabelCount(g.Key, g.Count()))
            .OrderByDescending(x => x.Count)
            .ToArray();

        var bySeverity = incidents
            .GroupBy(i => i.Severity)
            .Select(g => new LabelCount(g.Key, g.Count()))
            .OrderByDescending(x => x.Count)
            .ToArray();

        return new QualitySummary(open, underInv, resolvedThisMonth,
            feedback.Count, Math.Round(avgRating, 1), byType, bySeverity);
    }

    private static QualityIncidentResponse ToResponse(QualityIncident q) => new(
        q.Id, q.Title, q.Description, q.IncidentType, q.Severity, q.Status,
        q.ReportedByUserId, q.ReportedByUser?.Username ?? string.Empty,
        q.AssignedToUserId, q.AssignedToUser?.Username,
        q.PatientId,
        q.Patient is null ? null : $"{q.Patient.FirstName} {q.Patient.LastName}",
        q.Location, q.RootCause, q.CorrectiveAction,
        q.OccurredAtUtc, q.ReportedAtUtc, q.ResolvedAtUtc);

    private static PatientFeedbackResponse ToFeedbackResponse(PatientFeedback f, Patient? patient) => new(
        f.Id, f.PatientId,
        patient is null ? string.Empty : $"{patient.FirstName} {patient.LastName}",
        f.AppointmentId, f.OverallRating, f.StaffRating, f.FacilityRating,
        f.Comments, f.SubmittedAtUtc);
}
