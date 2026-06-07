using HospitalMS.Business.Models;
using HospitalMS.Business.Repositories;
using HospitalMS.Business.Services;
using HospitalMS.Common.Auth;
using HospitalMS.Common.Constants;
using HospitalMS.Data.Persistence;
using HospitalMS.Data.Persistence.Entities;
using Microsoft.Extensions.Options;

namespace HospitalMS.Tests.UnitTests;

public sealed class UserServiceTests : IntegrationTestBase
{
    private static readonly IOptions<JwtSettings> JwtOptions = Options.Create(new JwtSettings
    {
        Secret = "UnitTestJwtSecretKeyThatIsLongEnough123",
        Issuer = "Test",
        Audience = "Test",
        ExpiryMinutes = 60
    });

    private UserService CreateService(HospitalDbContext ctx)
        => new(new EfRepository<User>(ctx), ctx, new PasswordHasher(), new JwtTokenProvider(JwtOptions), JwtOptions);

    [Fact]
    public async Task RegisterAsync_CreatesUserAndReturnsToken()
    {
        await using var ctx = CreateContext();
        var service = CreateService(ctx);

        var result = await service.RegisterAsync(
            new RegisterRequest("dr.test", "dr@test.com", "Password1!", UserRoles.Doctor));

        Assert.NotNull(result.Token);
        Assert.Equal("dr.test", result.Username);
        Assert.Equal(UserRoles.Doctor, result.Role);
        Assert.True(result.ExpiresAt > DateTime.UtcNow);
    }

    [Fact]
    public async Task RegisterAsync_ThrowsArgumentException_WhenRoleIsInvalid()
    {
        await using var ctx = CreateContext();
        var service = CreateService(ctx);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.RegisterAsync(new RegisterRequest("user", "u@test.com", "Pass1!", "InvalidRole")));
    }

    [Fact]
    public async Task RegisterAsync_ThrowsInvalidOperationException_WhenUsernameAlreadyTaken()
    {
        var existing = TestFixtures.CreateUser(username: "taken");
        Context.Users.Add(existing);
        await Context.SaveChangesAsync();

        await using var ctx = CreateContext();
        var service = CreateService(ctx);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.RegisterAsync(new RegisterRequest("taken", "new@test.com", "Pass1!", UserRoles.Nurse)));
    }

    [Fact]
    public async Task LoginAsync_ThrowsUnauthorizedAccessException_WhenUserNotFound()
    {
        await using var ctx = CreateContext();
        var service = CreateService(ctx);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.LoginAsync(new LoginRequest("nobody", "pass")));
    }

    [Fact]
    public async Task LoginAsync_ThrowsUnauthorizedAccessException_WhenPasswordIsWrong()
    {
        var hasher = new PasswordHasher();
        var existing = TestFixtures.CreateUser(username: "doctor1");
        existing.PasswordHash = hasher.HashPassword("CorrectPass!");
        Context.Users.Add(existing);
        await Context.SaveChangesAsync();

        await using var ctx = CreateContext();
        var service = CreateService(ctx);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.LoginAsync(new LoginRequest("doctor1", "WrongPass!")));
    }

    [Fact]
    public async Task LoginAsync_ReturnsToken_WithCorrectCredentials()
    {
        var hasher = new PasswordHasher();
        var existing = TestFixtures.CreateUser(username: "nurse1", role: UserRoles.Nurse);
        existing.PasswordHash = hasher.HashPassword("CorrectPass!");
        Context.Users.Add(existing);
        await Context.SaveChangesAsync();

        await using var ctx = CreateContext();
        var service = CreateService(ctx);

        var result = await service.LoginAsync(new LoginRequest("nurse1", "CorrectPass!"));

        Assert.NotNull(result.Token);
        Assert.Equal("nurse1", result.Username);
        Assert.Equal(UserRoles.Nurse, result.Role);
    }
}
