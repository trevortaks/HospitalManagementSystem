namespace HospitalMS.Business.Models;

public sealed record UserSummary(
    Guid Id,
    string Username,
    string Email,
    string Role,
    bool IsActive,
    DateTime CreatedAtUtc,
    string? FirstName = null,
    string? LastName = null,
    string? PhoneNumber = null,
    string? AddressLine1 = null,
    string? City = null,
    string? PostalCode = null,
    string? Country = null,
    string? Specialization = null,
    string? LicenseNumber = null,
    string? Bio = null);

public sealed record UpdateUserProfileRequest(
    string? FirstName = null,
    string? LastName = null,
    string? PhoneNumber = null,
    string? AddressLine1 = null,
    string? City = null,
    string? PostalCode = null,
    string? Country = null,
    string? Specialization = null,
    string? LicenseNumber = null,
    string? Bio = null);
