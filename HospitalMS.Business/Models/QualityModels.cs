namespace HospitalMS.Business.Models;

public record QualityIncidentResponse(
    Guid Id,
    string Title,
    string Description,
    string IncidentType,
    string Severity,
    string Status,
    Guid ReportedByUserId,
    string ReportedByUsername,
    Guid? AssignedToUserId,
    string? AssignedToUsername,
    Guid? PatientId,
    string? PatientName,
    string? Location,
    string? RootCause,
    string? CorrectiveAction,
    DateTime OccurredAtUtc,
    DateTime ReportedAtUtc,
    DateTime? ResolvedAtUtc);

public record CreateQualityIncidentRequest(
    string Title,
    string Description,
    string IncidentType,
    string Severity,
    Guid ReportedByUserId,
    Guid? PatientId = null,
    string? Location = null,
    DateTime? OccurredAtUtc = null);

public record AssignQualityIncidentRequest(Guid AssignedToUserId);

public record InvestigateQualityIncidentRequest(string? RootCause, string? CorrectiveAction);

public record ResolveQualityIncidentRequest(string? ResolutionNotes = null);

public record PatientFeedbackResponse(
    Guid Id,
    Guid PatientId,
    string PatientName,
    Guid? AppointmentId,
    int OverallRating,
    int? StaffRating,
    int? FacilityRating,
    string? Comments,
    DateTime SubmittedAtUtc);

public record CreatePatientFeedbackRequest(
    Guid PatientId,
    int OverallRating,
    Guid? AppointmentId = null,
    int? StaffRating = null,
    int? FacilityRating = null,
    string? Comments = null);

public record QualitySummary(
    int OpenIncidents,
    int UnderInvestigationIncidents,
    int ResolvedThisMonth,
    int TotalFeedback,
    double AverageOverallRating,
    IReadOnlyList<LabelCount> IncidentsByType,
    IReadOnlyList<LabelCount> IncidentsBySeverity);
