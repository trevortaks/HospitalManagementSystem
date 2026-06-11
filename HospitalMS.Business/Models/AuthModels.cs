namespace HospitalMS.Business.Models;

public sealed record RegisterRequest(
    string Username,
    string Email,
    string Password,
    string Role);

public sealed record LoginRequest(
    string Username,
    string Password);

public sealed record AuthResponse(
    string Token,
    string Username,
    string Role,
    DateTime ExpiresAt,
    Guid UserId = default);
