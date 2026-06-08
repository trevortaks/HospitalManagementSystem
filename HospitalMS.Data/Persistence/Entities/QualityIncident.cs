namespace HospitalMS.Data.Persistence.Entities;

public sealed class QualityIncident
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string IncidentType { get; set; } = QualityIncidentType.AdverseEvent;
    public string Severity { get; set; } = QualityIncidentSeverity.Moderate;
    public string Status { get; set; } = QualityIncidentStatus.Open;
    public Guid ReportedByUserId { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public Guid? PatientId { get; set; }
    public string? Location { get; set; }
    public string? RootCause { get; set; }
    public string? CorrectiveAction { get; set; }
    public DateTime OccurredAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime ReportedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ResolvedAtUtc { get; set; }

    public User ReportedByUser { get; set; } = null!;
    public User? AssignedToUser { get; set; }
    public Patient? Patient { get; set; }
}

public static class QualityIncidentStatus
{
    public const string Open = "Open";
    public const string UnderInvestigation = "UnderInvestigation";
    public const string Resolved = "Resolved";
    public const string Closed = "Closed";
}

public static class QualityIncidentSeverity
{
    public const string Low = "Low";
    public const string Moderate = "Moderate";
    public const string High = "High";
    public const string Critical = "Critical";
}

public static class QualityIncidentType
{
    public const string AdverseEvent = "AdverseEvent";
    public const string NearMiss = "NearMiss";
    public const string Complaint = "Complaint";
    public const string Suggestion = "Suggestion";
    public static readonly string[] All = [AdverseEvent, NearMiss, Complaint, Suggestion];
}
