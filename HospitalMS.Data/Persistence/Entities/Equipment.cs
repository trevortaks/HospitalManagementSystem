namespace HospitalMS.Data.Persistence.Entities;

public sealed class Equipment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string EquipmentType { get; set; } = string.Empty;
    public string? SerialNumber { get; set; }
    public string? Manufacturer { get; set; }
    public string? Model { get; set; }
    public Guid? LocationRoomId { get; set; }
    public string Status { get; set; } = "Active";
    public DateTime? PurchaseDate { get; set; }
    public DateTime? LastMaintenanceDate { get; set; }
    public DateTime? NextMaintenanceDue { get; set; }
    public DateTime? WarrantyExpiryDate { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public Room? LocationRoom { get; set; }
    public ICollection<MaintenanceRequest> MaintenanceRequests { get; set; } = [];
}

public static class EquipmentStatus
{
    public const string Active           = "Active";
    public const string UnderMaintenance = "UnderMaintenance";
    public const string Decommissioned   = "Decommissioned";
}
