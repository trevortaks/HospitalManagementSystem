using HospitalMS.Business.Models;
using HospitalMS.Business.Repositories;
using HospitalMS.Business.Services;
using HospitalMS.Common.Auth;
using HospitalMS.Common.Constants;
using HospitalMS.Data.Persistence.Entities;
using Microsoft.Extensions.Options;

namespace HospitalMS.Tests.IntegrationTests;

public sealed class AuthFlowIntegrationTests : IntegrationTestBase
{
    private static readonly IOptions<JwtSettings> JwtOptions = Options.Create(new JwtSettings
    {
        Secret = "IntegrationTestJwtSecretKeyLongEnough12345",
        Issuer = "HospitalMS.Tests",
        Audience = "HospitalMS.TestClients",
        ExpiryMinutes = 30
    });

    [Fact]
    public async Task RegisterThenLogin_ReturnsValidTokenWithCorrectClaims()
    {
        await using var ctx = CreateContext();
        var hasher = new PasswordHasher();
        var jwtProvider = new JwtTokenProvider(JwtOptions);
        var userRepo = new EfRepository<User>(ctx);
        var service = new UserService(userRepo, ctx, hasher, jwtProvider, JwtOptions);

        var regResponse = await service.RegisterAsync(
            new RegisterRequest("nurse.flow", "nurse@test.com", "Secure!Pass1", UserRoles.Nurse));

        Assert.NotNull(regResponse.Token);
        Assert.Equal("nurse.flow", regResponse.Username);
        Assert.Equal(UserRoles.Nurse, regResponse.Role);

        var loginResponse = await service.LoginAsync(
            new LoginRequest("nurse.flow", "Secure!Pass1"));

        Assert.NotNull(loginResponse.Token);
        Assert.Equal(UserRoles.Nurse, loginResponse.Role);

        var principal = jwtProvider.ValidateToken(loginResponse.Token);
        Assert.Equal("nurse.flow", principal.Identity!.Name);
    }

    [Fact]
    public async Task Login_ThrowsUnauthorized_WithWrongPassword()
    {
        await using var ctx = CreateContext();
        var hasher = new PasswordHasher();
        var jwtProvider = new JwtTokenProvider(JwtOptions);
        var userRepo = new EfRepository<User>(ctx);
        var service = new UserService(userRepo, ctx, hasher, jwtProvider, JwtOptions);

        await service.RegisterAsync(
            new RegisterRequest("doc.wrong", "doc@test.com", "CorrectPass1!", UserRoles.Doctor));

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.LoginAsync(new LoginRequest("doc.wrong", "WrongPass!")));
    }

    [Fact]
    public async Task Register_ThrowsInvalidOperation_WhenUsernameAlreadyTaken()
    {
        await using var ctx = CreateContext();
        var hasher = new PasswordHasher();
        var jwtProvider = new JwtTokenProvider(JwtOptions);
        var userRepo = new EfRepository<User>(ctx);
        var service = new UserService(userRepo, ctx, hasher, jwtProvider, JwtOptions);

        await service.RegisterAsync(
            new RegisterRequest("duplicate.user", "d@test.com", "Pass1!", UserRoles.Admin));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.RegisterAsync(new RegisterRequest("duplicate.user", "d2@test.com", "Pass2!", UserRoles.Admin)));
    }
}
