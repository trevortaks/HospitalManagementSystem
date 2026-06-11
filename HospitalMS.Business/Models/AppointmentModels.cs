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

public sealed record RecordAppointmentVitalsRequest(
    decimal? HeightCm = null,
    decimal? WeightKg = null,
    decimal? TemperatureCelsius = null,
    int? BloodPressureSystolic = null,
    int? BloodPressureDiastolic = null,
    int? HeartRateBpm = null,
    int? RespiratoryRate = null,
    decimal? OxygenSaturationPct = null,
    string? Notes = null);

public sealed record AppointmentVitalsResponse(
    Guid Id,
    Guid AppointmentId,
    Guid RecordedByUserId,
    string RecordedByName,
    DateTime RecordedAtUtc,
    decimal? HeightCm,
    decimal? WeightKg,
    decimal? TemperatureCelsius,
    int? BloodPressureSystolic,
    int? BloodPressureDiastolic,
    int? HeartRateBpm,
    int? RespiratoryRate,
    decimal? OxygenSaturationPct,
    string? Notes);

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
    DateTime? UpdatedAtUtc,
    AppointmentVitalsResponse? PreConsultVitals = null);
