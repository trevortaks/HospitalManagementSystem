using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HospitalMS.Common.Constants;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace HospitalMS.Common.Auth;

public sealed class JwtTokenProvider(IOptions<JwtSettings> jwtOptions) : IJwtTokenProvider
{
    private readonly JwtSettings _settings = jwtOptions.Value;
    private readonly JwtSecurityTokenHandler _tokenHandler = new();

    public string GenerateToken(
        string userId,
        string userName,
        IEnumerable<string>? roles = null,
        IDictionary<string, string>? additionalClaims = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(userName);

        var issuedAt = DateTime.UtcNow;
        var normalizedRoles = roles?
            .Where(static role => !string.IsNullOrWhiteSpace(role))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray() ?? [];

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId),
            new(ClaimTypes.NameIdentifier, userId),
            new(JwtRegisteredClaimNames.UniqueName, userName),
            new(ClaimTypes.Name, userName),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        claims.AddRange(normalizedRoles.Select(role => new Claim(ClaimTypes.Role, role)));
        claims.AddRange(Permissions.GetPermissionsForRoles(normalizedRoles)
            .Select(permission => new Claim(Permissions.PermissionClaimType, permission)));

        if (additionalClaims is not null)
        {
            claims.AddRange(
                additionalClaims
                    .Where(static pair => !string.IsNullOrWhiteSpace(pair.Key) && !string.IsNullOrWhiteSpace(pair.Value))
                    .Select(pair => new Claim(pair.Key, pair.Value)));
        }

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Issuer = _settings.Issuer,
            Audience = _settings.Audience,
            NotBefore = issuedAt,
            Expires = issuedAt.AddMinutes(_settings.ExpiryMinutes),
            SigningCredentials = new SigningCredentials(GetSigningKey(_settings.Secret), SecurityAlgorithms.HmacSha256)
        };

        var token = _tokenHandler.CreateToken(descriptor);
        return _tokenHandler.WriteToken(token);
    }

    public ClaimsPrincipal ValidateToken(string token)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(token);

        return _tokenHandler.ValidateToken(token, CreateValidationParameters(_settings), out _);
    }

    public bool TryValidateToken(string token, out ClaimsPrincipal? principal)
    {
        principal = null;

        if (string.IsNullOrWhiteSpace(token))
        {
            return false;
        }

        try
        {
            principal = ValidateToken(token);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public IReadOnlyCollection<Claim> GetClaims(string token)
    {
        return ValidateToken(token).Claims.ToArray();
    }

    public static TokenValidationParameters CreateValidationParameters(JwtSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        return new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = GetSigningKey(settings.Secret),
            ValidateIssuer = true,
            ValidIssuer = settings.Issuer,
            ValidateAudience = true,
            ValidAudience = settings.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            NameClaimType = ClaimTypes.Name,
            RoleClaimType = ClaimTypes.Role
        };
    }

    private static SymmetricSecurityKey GetSigningKey(string secret)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(secret);
        return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
    }
}
