namespace HospitalMS.Data.Persistence.Entities;

public sealed class MaintenanceRequest
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string RequestType { get; set; } = "Corrective";
    public string Priority { get; set; } = "Medium";
    public string Status { get; set; } = "Open";
    public Guid? RoomId { get; set; }
    public Guid? EquipmentId { get; set; }
    public Guid RequestedByUserId { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public DateTime RequestedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ScheduledDate { get; set; }
    public DateTime? ResolvedAtUtc { get; set; }
    public string? ResolutionNotes { get; set; }

    public Room? Room { get; set; }
    public Equipment? Equipment { get; set; }
    public User RequestedByUser { get; set; } = null!;
    public User? AssignedToUser { get; set; }
}

public static class MaintenanceRequestStatus
{
    public const string Open       = "Open";
    public const string InProgress = "InProgress";
    public const string OnHold     = "OnHold";
    public const string Resolved   = "Resolved";
    public const string Closed     = "Closed";
}

public static class MaintenanceRequestPriority
{
    public const string Low      = "Low";
    public const string Medium   = "Medium";
    public const string High     = "High";
    public const string Critical = "Critical";
}

public static class MaintenanceRequestType
{
    public const string Corrective = "Corrective";
    public const string Preventive = "Preventive";
    public const string Emergency  = "Emergency";
    public const string Inspection = "Inspection";
}
