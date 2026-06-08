using HospitalMS.Business.Repositories;
using HospitalMS.Business.Services;
using HospitalMS.Common.Audit;
using HospitalMS.Common.Storage;
using HospitalMS.Data.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HospitalMS.Business;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHospitalDataAccess(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<HospitalDbContext>((serviceProvider, options) =>
        {
            var connectionString = configuration.GetConnectionString("hospitalms")
                ?? configuration.GetConnectionString("postgres")
                ?? configuration.GetConnectionString("HospitalDb");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                options.UseInMemoryDatabase("HospitalMS");
            }
            else
            {
                options.UseNpgsql(connectionString,
                    x => x.MigrationsAssembly("HospitalMS.Data"));
            }

            var auditInterceptor = serviceProvider.GetService<AuditInterceptor>();
            if (auditInterceptor is not null)
            {
                options.AddInterceptors(auditInterceptor);
            }
        });

        return services;
    }

    public static IServiceCollection AddHospitalBusinessServices(this IServiceCollection services)
    {
        services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
        services.AddScoped<IPatientService, PatientService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IAppointmentService, AppointmentService>();
        services.AddScoped<IEncounterService, EncounterService>();
        services.AddScoped<IMedicationService, MedicationService>();
        services.AddScoped<IPrescriptionService, PrescriptionService>();
        services.AddScoped<IPortalService, PortalService>();
        services.AddScoped<ILabService, LabService>();
        services.AddScoped<IImagingService, ImagingService>();
        services.AddScoped<IBillingService, BillingService>();
        services.AddScoped<IInsuranceService, InsuranceService>();
        services.AddSingleton<IFileStorageService, LocalFileStorageService>();

        services.AddHealthChecks()
            .AddCheck<HospitalDatabaseHealthCheck>("postgres", tags: ["ready"]);

        return services;
    }

    private sealed class HospitalDatabaseHealthCheck(IServiceScopeFactory scopeFactory) : IHealthCheck
    {
        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<HospitalDbContext>();
            var canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);
            return canConnect
                ? HealthCheckResult.Healthy("PostgreSQL connection is available.")
                : HealthCheckResult.Unhealthy("PostgreSQL connection is unavailable.");
        }
    }
}
