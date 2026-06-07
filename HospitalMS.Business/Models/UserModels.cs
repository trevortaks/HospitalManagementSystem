namespace HospitalMS.Business.Models;

public sealed record UserSummary(
    Guid Id,
    string Username,
    string Email,
    string Role,
    bool IsActive,
    DateTime CreatedAtUtc);
