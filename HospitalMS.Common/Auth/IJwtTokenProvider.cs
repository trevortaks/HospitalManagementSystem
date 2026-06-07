using System.Security.Claims;

namespace HospitalMS.Common.Auth;

public interface IJwtTokenProvider
{
    string GenerateToken(
        string userId,
        string userName,
        IEnumerable<string>? roles = null,
        IDictionary<string, string>? additionalClaims = null);

    ClaimsPrincipal ValidateToken(string token);

    bool TryValidateToken(string token, out ClaimsPrincipal? principal);

    IReadOnlyCollection<Claim> GetClaims(string token);
}
