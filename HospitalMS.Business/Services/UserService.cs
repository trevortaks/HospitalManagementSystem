using HospitalMS.Business.Models;
using HospitalMS.Business.Repositories;
using HospitalMS.Common.Auth;
using HospitalMS.Common.Constants;
using HospitalMS.Data.Persistence;
using HospitalMS.Data.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace HospitalMS.Business.Services;

public sealed class UserService(
    IRepository<User> repository,
    HospitalDbContext dbContext,
    IPasswordHasher passwordHasher,
    IJwtTokenProvider jwtTokenProvider,
    IOptions<JwtSettings> jwtOptions) : IUserService
{
    private readonly JwtSettings _jwtSettings = jwtOptions.Value;

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        if (!UserRoles.IsValid(request.Role))
            throw new ArgumentException($"'{request.Role}' is not a valid role.", nameof(request));

        var existing = await dbContext.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(u => u.Username == request.Username, cancellationToken);

        if (existing is not null)
            throw new InvalidOperationException($"Username '{request.Username}' is already taken.");

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            Role = request.Role,
            PasswordHash = passwordHasher.HashPassword(request.Password),
            IsActive = true
        };

        await repository.AddAsync(user, cancellationToken);
        return BuildAuthResponse(user);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await dbContext.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(u => u.Username == request.Username && u.IsActive, cancellationToken)
            ?? throw new UnauthorizedAccessException("Invalid username or password.");

        if (!passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid username or password.");

        return BuildAuthResponse(user);
    }

    public async Task<IReadOnlyList<UserSummary>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var users = await dbContext.Users
            .AsNoTracking()
            .OrderByDescending(u => u.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        return users.Select(ToSummary).ToArray();
    }

    public async Task<UserSummary?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await dbContext.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(u => u.Id == id, cancellationToken);
        return user is null ? null : ToSummary(user);
    }

    public async Task<UserSummary> UpdateProfileAsync(Guid id, UpdateUserProfileRequest request, CancellationToken cancellationToken = default)
    {
        var user = await dbContext.Users.FindAsync([id], cancellationToken)
            ?? throw new KeyNotFoundException($"User {id} not found.");

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.PhoneNumber = request.PhoneNumber;
        user.AddressLine1 = request.AddressLine1;
        user.City = request.City;
        user.PostalCode = request.PostalCode;
        user.Country = request.Country;
        user.Specialization = request.Specialization;
        user.LicenseNumber = request.LicenseNumber;
        user.Bio = request.Bio;

        await dbContext.SaveChangesAsync(cancellationToken);
        return ToSummary(user);
    }

    public async Task<UserSummary> ToggleActiveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await dbContext.Users.FindAsync([id], cancellationToken)
            ?? throw new KeyNotFoundException($"User {id} not found.");
        user.IsActive = !user.IsActive;
        await dbContext.SaveChangesAsync(cancellationToken);
        return ToSummary(user);
    }

    private static UserSummary ToSummary(User u) => new(
        u.Id, u.Username, u.Email, u.Role, u.IsActive, u.CreatedAtUtc,
        u.FirstName, u.LastName, u.PhoneNumber,
        u.AddressLine1, u.City, u.PostalCode, u.Country,
        u.Specialization, u.LicenseNumber, u.Bio);

    private AuthResponse BuildAuthResponse(User user)
    {
        var token = jwtTokenProvider.GenerateToken(
            userId: user.Id.ToString(),
            userName: user.Username,
            roles: [user.Role]);

        return new AuthResponse(
            Token: token,
            Username: user.Username,
            Role: user.Role,
            ExpiresAt: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes));
    }
}
