using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace HospitalMS.Common.Audit;

public sealed class AuditLogger(ILogger<AuditLogger> logger, IHttpContextAccessor httpContextAccessor) : IAuditLogger
{
    public Task LogAsync(AuditLog auditLog, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var enrichedLog = Enrich(auditLog);

        logger.LogInformation(
            "AUDIT UserId={UserId} Action={Action} Resource={Resource} Timestamp={Timestamp:o} IpAddress={IpAddress} Details={Details}",
            enrichedLog.UserId ?? "anonymous",
            enrichedLog.Action,
            enrichedLog.Resource,
            enrichedLog.Timestamp,
            enrichedLog.IpAddress ?? "unknown",
            enrichedLog.Details ?? string.Empty);

        return Task.CompletedTask;
    }

    public Task LogAsync(
        string action,
        string resource,
        string? details = null,
        string? userId = null,
        string? ipAddress = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(action);
        ArgumentException.ThrowIfNullOrWhiteSpace(resource);

        return LogAsync(
            new AuditLog
            {
                Action = action,
                Resource = resource,
                Details = details,
                UserId = userId,
                IpAddress = ipAddress
            },
            cancellationToken);
    }

    private AuditLog Enrich(AuditLog auditLog)
    {
        var httpContext = httpContextAccessor.HttpContext;
        var requestPath = httpContext?.Request.Path.Value;
        var requestMethod = httpContext?.Request.Method;
        var contextualDetails = string.IsNullOrWhiteSpace(requestPath)
            ? auditLog.Details
            : string.IsNullOrWhiteSpace(auditLog.Details)
                ? $"{requestMethod} {requestPath}"
                : $"{auditLog.Details} | {requestMethod} {requestPath}";

        return new AuditLog
        {
            Action = auditLog.Action,
            Resource = string.IsNullOrWhiteSpace(auditLog.Resource) ? requestPath ?? "unknown-resource" : auditLog.Resource,
            Details = contextualDetails,
            Timestamp = auditLog.Timestamp == default ? DateTimeOffset.UtcNow : auditLog.Timestamp,
            UserId = auditLog.UserId ?? httpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier),
            IpAddress = auditLog.IpAddress ?? httpContext?.Connection.RemoteIpAddress?.ToString()
        };
    }
}
