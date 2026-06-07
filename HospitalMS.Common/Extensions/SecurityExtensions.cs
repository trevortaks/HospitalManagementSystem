using System.ComponentModel.DataAnnotations;
using HospitalMS.Common.Audit;
using HospitalMS.Common.Auth;
using HospitalMS.Common.Constants;
using HospitalMS.Common.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace HospitalMS.Common.Extensions;

public static class SecurityExtensions
{
    public static IServiceCollection AddHospitalSecurity(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddSecurityHeaders(configuration);

        services
            .AddOptions<JwtSettings>()
            .Bind(configuration.GetSection(JwtSettings.SectionName))
            .ValidateDataAnnotations()
            .Validate(
                static settings => !string.IsNullOrWhiteSpace(settings.Secret) && settings.Secret.Length >= 32,
                "JWT secret must be at least 32 characters long.")
            .ValidateOnStart();

        services.AddSingleton<IJwtTokenProvider, JwtTokenProvider>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IAuditLogger, AuditLogger>();
        services.AddScoped<AuditInterceptor>();

        var jwtSettings = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>() ?? new JwtSettings();
        Validator.ValidateObject(jwtSettings, new ValidationContext(jwtSettings), validateAllProperties: true);

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = true;
                options.SaveToken = true;
                options.TokenValidationParameters = JwtTokenProvider.CreateValidationParameters(jwtSettings);
            });

        services.AddAuthorization(options =>
        {
            foreach (var permission in Permissions.All)
            {
                options.AddPolicy(permission, policy => policy.RequireClaim(Permissions.PermissionClaimType, permission));
            }

            foreach (var role in UserRoles.All)
            {
                options.AddPolicy($"{UserRoles.PolicyPrefix}{role}", policy => policy.RequireRole(role));
            }
        });

        return services;
    }

    public static IApplicationBuilder UseHospitalSecurity(this IApplicationBuilder app)
    {
        app.UseSecurityHeaders();
        app.UseAuthentication();
        app.UseAuthorization();
        return app;
    }
}
