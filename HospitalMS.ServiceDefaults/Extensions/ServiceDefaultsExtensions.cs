using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HospitalMS.ServiceDefaults.Extensions;

/// <summary>
/// Centralises the cross-cutting configuration that every Hospital Management service should share.
/// Usage: call <c>builder.AddServiceDefaults()</c> during startup and <c>app.MapDefaultHealthChecks()</c> after building the app.
/// </summary>
public static class ServiceDefaultsExtensions
{
    private const string ConfigurationSectionName = "ServiceDefaults";
    private const string ReadinessTag = "ready";
    private const string HealthEndpointPath = "/health";

    /// <summary>
    /// Adds the shared infrastructure used by all hosted services: logging, telemetry, health checks, and service discovery.
    /// Environment variables prefixed with <c>HOSPITALMS_</c> override values under the <c>ServiceDefaults</c> configuration section.
    /// </summary>
    public static TBuilder AddServiceDefaults<TBuilder>(this TBuilder builder)
        where TBuilder : IHostApplicationBuilder
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Configuration.AddEnvironmentVariables(prefix: "HOSPITALMS_");

        ConfigureLogging(builder);
        ConfigureHealthChecks(builder.Services, builder.Environment);
        ConfigureServiceDiscovery(builder.Services);

        return builder;
    }

    /// <summary>
    /// Maps the shared readiness endpoint used by Aspire, container orchestrators, and local diagnostics.
    /// </summary>
    public static WebApplication MapDefaultHealthChecks(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.MapHealthChecks(HealthEndpointPath, new HealthCheckOptions
        {
            // Only readiness-tagged checks are surfaced here so dependent services only connect when this service is ready.
            Predicate = registration => registration.Tags.Contains(ReadinessTag),
            AllowCachingResponses = false
        });

        return app;
    }

    private static void ConfigureLogging(IHostApplicationBuilder builder)
    {
        var loggingSection = builder.Configuration.GetSection($"{ConfigurationSectionName}:Logging");
        var structuredLoggingEnabled = loggingSection.GetValue<bool?>("StructuredConsoleEnabled")
            ?? !builder.Environment.IsDevelopment();
        var defaultLogLevel = GetDefaultLogLevel(builder.Environment);
        var configuredLogLevel = loggingSection.GetValue("MinimumLevel", defaultLogLevel);

        builder.Logging.ClearProviders();
        builder.Logging.AddConfiguration(loggingSection);

        if (structuredLoggingEnabled)
        {
            // JSON console logs are easy to ship to central log stores and preserve structured fields.
            builder.Logging.AddJsonConsole(options =>
            {
                options.IncludeScopes = true;
                options.TimestampFormat = "O";
                options.UseUtcTimestamp = true;
            });
        }
        else
        {
            // Keep development logs compact and readable when working locally.
            builder.Logging.AddSimpleConsole(options =>
            {
                options.IncludeScopes = true;
                options.SingleLine = true;
                options.TimestampFormat = "HH:mm:ss ";
            });
        }

        builder.Logging.SetMinimumLevel(configuredLogLevel);
    }

    private static void ConfigureHealthChecks(IServiceCollection services, IHostEnvironment environment)
    {
        services.AddHealthChecks()
            // Self confirms the process is healthy and ready to accept traffic.
            .AddCheck(
                "self",
                () => HealthCheckResult.Healthy($"{environment.ApplicationName} is running."),
                tags: new[] { ReadinessTag })
            // Configuration confirms the shared defaults loaded for the active environment.
            .AddCheck(
                "configuration",
                () => HealthCheckResult.Healthy($"Service defaults loaded for {environment.EnvironmentName}."),
                tags: new[] { ReadinessTag });
    }

    private static void ConfigureServiceDiscovery(IServiceCollection services)
    {
        services.AddServiceDiscovery();

        // Any HttpClient registered by a service can now resolve named endpoints through Aspire service discovery.
        services.ConfigureHttpClientDefaults(http => http.AddServiceDiscovery());
    }

    private static LogLevel GetDefaultLogLevel(IHostEnvironment environment) =>
        environment.IsDevelopment()
            ? LogLevel.Information
            : environment.IsStaging()
                ? LogLevel.Warning
                : LogLevel.Error;
}
