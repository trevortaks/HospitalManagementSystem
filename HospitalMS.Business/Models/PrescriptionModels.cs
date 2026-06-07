namespace HospitalMS.Business.Models;

public sealed record CreatePrescriptionRequest(
    Guid EncounterId,
    Guid PatientId,
    Guid PrescribedByUserId,
    Guid MedicationId,
    string Dose,
    string Frequency,
    int? DurationDays = null,
    string? Instructions = null);

public sealed record DispensePrescriptionRequest(
    int QuantityDispensed,
    Guid DispensedByUserId);

public sealed record PrescriptionResponse(
    Guid Id,
    Guid EncounterId,
    Guid PatientId,
    string PatientName,
    string PatientMrn,
    Guid PrescribedByUserId,
    string PrescribedByName,
    Guid MedicationId,
    string MedicationGenericName,
    string? MedicationBrandName,
    string MedicationForm,
    string? Strength,
    string? RouteOfAdministration,
    string Dose,
    string Frequency,
    int? DurationDays,
    int? QuantityDispensed,
    string? Instructions,
    string Status,
    bool IsControlled,
    DateTime PrescribedAtUtc,
    DateTime? DispensedAtUtc,
    string? DispensedByName);
