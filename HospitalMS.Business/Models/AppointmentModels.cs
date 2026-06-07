namespace HospitalMS.Business.Models;

public sealed record CreateAppointmentRequest(
    Guid PatientId,
    Guid DoctorUserId,
    DateTime ScheduledAtUtc,
    int DurationMinutes = 30,
    string Type = "General",
    string? Reason = null,
    string? Notes = null);

public sealed record UpdateAppointmentRequest(
    DateTime ScheduledAtUtc,
    int DurationMinutes,
    string Type,
    string? Reason,
    string? Notes);

public sealed record CancelAppointmentRequest(string? Reason);

public sealed record PatchAppointmentStatusRequest(string Status);

public sealed record AppointmentResponse(
    Guid Id,
    Guid PatientId,
    string PatientName,
    string PatientMrn,
    Guid DoctorUserId,
    string DoctorName,
    DateTime ScheduledAtUtc,
    int DurationMinutes,
    string Status,
    string Type,
    string? Reason,
    string? Notes,
    string? CancelledReason,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);
