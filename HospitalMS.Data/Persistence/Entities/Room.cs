namespace HospitalMS.Data.Persistence.Entities;

public sealed class Room
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string RoomNumber { get; set; } = string.Empty;
    public string RoomType { get; set; } = "General";
    public int FloorNumber { get; set; } = 1;
    public string? Building { get; set; }
    public int CapacityPersons { get; set; } = 1;
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<MaintenanceRequest> MaintenanceRequests { get; set; } = [];
}

public static class RoomType
{
    public const string General          = "General";
    public const string OperatingTheatre = "OperatingTheatre";
    public const string ConsultationRoom = "ConsultationRoom";
    public const string ProcedureRoom    = "ProcedureRoom";
    public const string Laboratory       = "Laboratory";
    public const string ConferenceRoom   = "ConferenceRoom";
    public const string Reception        = "Reception";
    public const string Storage          = "Storage";
    public const string Utility          = "Utility";
}
