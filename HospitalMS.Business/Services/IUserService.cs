using HospitalMS.Business.Models;

namespace HospitalMS.Business.Services;

public interface IUserService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserSummary>> GetAllAsync(CancellationToken cancellationToken = default);
}
