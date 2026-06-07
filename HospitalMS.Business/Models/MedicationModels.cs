namespace HospitalMS.Business.Models;

public sealed record CreateMedicationRequest(
    string GenericName,
    string Form,
    string? BrandName = null,
    string? Strength = null,
    string? RouteOfAdministration = null,
    bool IsControlled = false,
    bool IsActive = true);

public sealed record UpdateMedicationRequest(
    string GenericName,
    string Form,
    string? BrandName = null,
    string? Strength = null,
    string? RouteOfAdministration = null,
    bool IsControlled = false,
    bool IsActive = true);

public sealed record MedicationResponse(
    Guid Id,
    string GenericName,
    string? BrandName,
    string Form,
    string? Strength,
    string? RouteOfAdministration,
    bool IsControlled,
    bool IsActive,
    DateTime CreatedAtUtc);
