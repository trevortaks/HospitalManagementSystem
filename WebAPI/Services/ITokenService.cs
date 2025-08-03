using HospitalManagementSystem.Infrastructure.Identity;

namespace HospitalManagementSystem.WebAPI.Services;

public interface ITokenService
{
    Task<string> CreateToken(ApplicationUser user);
}
