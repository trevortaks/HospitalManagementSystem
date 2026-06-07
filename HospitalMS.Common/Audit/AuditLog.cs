namespace HospitalMS.Common.Audit;

public sealed class AuditLog
{
    public string? UserId { get; init; }

    public string Action { get; init; } = string.Empty;

    public string Resource { get; init; } = string.Empty;

    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;

    public string? Details { get; init; }

    public string? IpAddress { get; init; }
}
