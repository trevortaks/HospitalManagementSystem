namespace HospitalMS.Business.Models;

public sealed record PortalProfileResponse(
    Guid PatientId,
    string FirstName,
    string LastName,
    string MedicalRecordNumber,
    DateTime DateOfBirth,
    string? Email,
    string? PhoneNumber,
    string? Gender,
    string? BloodGroup,
    string? AddressLine1,
    string? City,
    string? PostalCode,
    string? Country,
    string? EmergencyContactName,
    string? EmergencyContactPhone);

public sealed record UpdatePortalProfileRequest(
    string? PhoneNumber,
    string? Gender,
    string? BloodGroup,
    string? AddressLine1,
    string? City,
    string? PostalCode,
    string? Country,
    string? EmergencyContactName,
    string? EmergencyContactPhone);

public sealed record PortalAppointmentResponse(
    Guid Id,
    DateTime ScheduledAtUtc,
    int DurationMinutes,
    string Status,
    string Type,
    string? Reason,
    string? Notes,
    string DoctorName);

public sealed record PortalEncounterResponse(
    Guid Id,
    DateTime StartedAtUtc,
    DateTime? EndedAtUtc,
    string EncounterType,
    string? ChiefComplaint,
    string? Assessment,
    string? Plan,
    string? FollowUpNotes,
    bool IsClosed,
    string DoctorName);

public sealed record PortalPrescriptionResponse(
    Guid Id,
    string MedicationGenericName,
    string? MedicationBrandName,
    string MedicationForm,
    string? Strength,
    string? RouteOfAdministration,
    string Dose,
    string Frequency,
    int? DurationDays,
    string? Instructions,
    string Status,
    bool IsControlled,
    DateTime PrescribedAtUtc,
    DateTime? DispensedAtUtc);

public sealed record PortalDashboardResponse(
    PortalProfileResponse Profile,
    IReadOnlyList<PortalAppointmentResponse> UpcomingAppointments,
    IReadOnlyList<PortalPrescriptionResponse> ActivePrescriptions,
    int TotalEncounters);

public sealed record LogPortalSessionRequest(
    Guid UserId,
    Guid PatientId,
    string? IpAddress,
    string? UserAgent);
