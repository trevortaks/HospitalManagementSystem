namespace HospitalMS.Business.Models;

public sealed record CreatePatientRequest(
    string MedicalRecordNumber,
    string FirstName,
    string LastName,
    DateTime DateOfBirth,
    string Email,
    string? PhoneNumber = null,
    string? Gender = null,
    string? BloodGroup = null,
    string? AddressLine1 = null,
    string? City = null,
    string? PostalCode = null,
    string? Country = null,
    string? EmergencyContactName = null,
    string? EmergencyContactPhone = null);

public sealed record UpdatePatientRequest(
    string FirstName,
    string LastName,
    DateTime DateOfBirth,
    string Email,
    string? PhoneNumber = null,
    string? Gender = null,
    string? BloodGroup = null,
    string? AddressLine1 = null,
    string? City = null,
    string? PostalCode = null,
    string? Country = null,
    string? EmergencyContactName = null,
    string? EmergencyContactPhone = null);

public sealed record PatientResponse(
    Guid Id,
    string MedicalRecordNumber,
    string FirstName,
    string LastName,
    DateTime DateOfBirth,
    string Email,
    string? PhoneNumber,
    string? Gender,
    string? BloodGroup,
    string? AddressLine1,
    string? City,
    string? PostalCode,
    string? Country,
    string? EmergencyContactName,
    string? EmergencyContactPhone,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);
