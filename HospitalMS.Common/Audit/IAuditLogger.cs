namespace HospitalMS.Common.Audit;

public interface IAuditLogger
{
    Task LogAsync(AuditLog auditLog, CancellationToken cancellationToken = default);

    Task LogAsync(
        string action,
        string resource,
        string? details = null,
        string? userId = null,
        string? ipAddress = null,
        CancellationToken cancellationToken = default);
}
