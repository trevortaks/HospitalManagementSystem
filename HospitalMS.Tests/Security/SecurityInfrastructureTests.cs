using System.Security.Claims;
using HospitalMS.Common.Auth;
using HospitalMS.Common.Constants;
using Microsoft.Extensions.Options;

namespace HospitalMS.Tests.Security;

public class SecurityInfrastructureTests
{
    private static readonly JwtSettings JwtSettings = new()
    {
        Secret = "UnitTestJwtSecretKeyThatIsLongEnough123456",
        Issuer = "HospitalMS.Tests",
        Audience = "HospitalMS.TestClients",
        ExpiryMinutes = 30
    };

    [Fact]
    public void PasswordHasher_HashesAndVerifiesPassword()
    {
        var passwordHasher = new PasswordHasher();

        var hash = passwordHasher.HashPassword("S3cure!Password");

        Assert.NotEqual("S3cure!Password", hash);
        Assert.True(passwordHasher.VerifyPassword("S3cure!Password", hash));
        Assert.False(passwordHasher.VerifyPassword("WrongPassword", hash));
    }

    [Fact]
    public void JwtTokenProvider_GeneratesAndValidatesToken_WithRolesAndPermissions()
    {
        var tokenProvider = new JwtTokenProvider(Options.Create(JwtSettings));

        var token = tokenProvider.GenerateToken(
            userId: "user-123",
            userName: "dr.house",
            roles: [UserRoles.Doctor],
            additionalClaims: new Dictionary<string, string>
            {
                ["department"] = "Cardiology"
            });

        var principal = tokenProvider.ValidateToken(token);

        Assert.Equal("user-123", principal.FindFirstValue(ClaimTypes.NameIdentifier));
        Assert.Equal("dr.house", principal.Identity?.Name);
        Assert.Contains(principal.Claims, claim => claim.Type == ClaimTypes.Role && claim.Value == UserRoles.Doctor);
        Assert.Contains(principal.Claims, claim => claim.Type == Permissions.PermissionClaimType && claim.Value == Permissions.PrescriptionsManage);
        Assert.Contains(tokenProvider.GetClaims(token), claim => claim.Type == "department" && claim.Value == "Cardiology");
        Assert.True(tokenProvider.TryValidateToken(token, out var validatedPrincipal));
        Assert.NotNull(validatedPrincipal);
    }

    [Fact]
    public void UserRoles_HasPermission_UsesRoleMappings()
    {
        Assert.True(UserRoles.HasPermission(UserRoles.Admin, Permissions.SystemAdmin));
        Assert.True(UserRoles.HasPermission([UserRoles.Nurse, UserRoles.Receptionist], Permissions.AppointmentsManage));
        Assert.False(UserRoles.HasPermission(UserRoles.Patient, Permissions.UsersManage));
    }
}
