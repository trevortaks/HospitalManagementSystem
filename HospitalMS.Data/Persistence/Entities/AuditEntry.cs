using HospitalMS.Common.Audit;

namespace HospitalMS.Data.Persistence.Entities;

public sealed class AuditEntry : INoAuditLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string EntityType { get; set; } = string.Empty;
    public Guid? EntityId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? Details { get; set; }
    public Guid? PerformedByUserId { get; set; }
    public string? PerformedByUsername { get; set; }
    public string? IpAddress { get; set; }
    public DateTime PerformedAtUtc { get; set; } = DateTime.UtcNow;
}

public static class AuditAction
{
    public const string Created       = "Created";
    public const string Updated       = "Updated";
    public const string Deleted       = "Deleted";
    public const string Viewed        = "Viewed";
    public const string LoggedIn      = "LoggedIn";
    public const string LoggedOut     = "LoggedOut";
    public const string StatusChanged = "StatusChanged";
    public const string AccessDenied  = "AccessDenied";
    public const string Exported      = "Exported";
}
