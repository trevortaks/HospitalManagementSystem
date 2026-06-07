namespace HospitalMS.Business.Models;

public sealed record CreateEncounterRequest(
    Guid PatientId,
    Guid AttendingDoctorId,
    string EncounterType,
    Guid? AppointmentId = null,
    string? ChiefComplaint = null,
    string? HistoryOfPresentIllness = null,
    string? Examination = null,
    string? Assessment = null,
    string? Plan = null);

public sealed record UpdateEncounterRequest(
    string EncounterType,
    string? ChiefComplaint,
    string? HistoryOfPresentIllness,
    string? Examination,
    string? Assessment,
    string? Plan,
    string? FollowUpNotes);

public sealed record AddDiagnosisRequest(
    string IcdCode,
    string Description,
    string DiagnosisType = "Primary");

public sealed record AddVitalsRequest(
    decimal? HeightCm = null,
    decimal? WeightKg = null,
    decimal? TemperatureCelsius = null,
    int? BloodPressureSystolic = null,
    int? BloodPressureDiastolic = null,
    int? HeartRateBpm = null,
    int? RespiratoryRate = null,
    decimal? OxygenSaturationPct = null,
    string? Notes = null);

public sealed record DiagnosisResponse(
    Guid Id,
    Guid EncounterId,
    string IcdCode,
    string Description,
    string DiagnosisType,
    DateTime CreatedAtUtc);

public sealed record VitalsResponse(
    Guid Id,
    Guid EncounterId,
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

public sealed record EncounterResponse(
    Guid Id,
    Guid PatientId,
    string PatientName,
    string PatientMrn,
    Guid? AppointmentId,
    Guid AttendingDoctorId,
    string AttendingDoctorName,
    string EncounterType,
    DateTime StartedAtUtc,
    DateTime? EndedAtUtc,
    string? ChiefComplaint,
    string? HistoryOfPresentIllness,
    string? Examination,
    string? Assessment,
    string? Plan,
    string? FollowUpNotes,
    bool IsClosed,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc,
    IReadOnlyList<DiagnosisResponse> Diagnoses,
    IReadOnlyList<VitalsResponse> VitalSigns);
