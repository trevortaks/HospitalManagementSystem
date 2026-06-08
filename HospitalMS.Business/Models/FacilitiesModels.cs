namespace HospitalMS.Business.Models;

public record RoomResponse(
    Guid Id,
    string Name,
    string RoomNumber,
    string RoomType,
    int FloorNumber,
    string? Building,
    int CapacityPersons,
    bool IsActive,
    string? Notes,
    DateTime CreatedAtUtc,
    int OpenRequestCount);

public record CreateRoomRequest(
    string Name,
    string RoomNumber,
    string RoomType,
    int FloorNumber = 1,
    string? Building = null,
    int CapacityPersons = 1,
    string? Notes = null);

public record EquipmentResponse(
    Guid Id,
    string Name,
    string Code,
    string EquipmentType,
    string? SerialNumber,
    string? Manufacturer,
    string? Model,
    Guid? LocationRoomId,
    string? LocationRoomName,
    string Status,
    DateTime? PurchaseDate,
    DateTime? LastMaintenanceDate,
    DateTime? NextMaintenanceDue,
    DateTime? WarrantyExpiryDate,
    string? Notes,
    bool IsActive,
    DateTime CreatedAtUtc);

public record CreateEquipmentRequest(
    string Name,
    string Code,
    string EquipmentType,
    string? SerialNumber = null,
    string? Manufacturer = null,
    string? Model = null,
    Guid? LocationRoomId = null,
    DateTime? PurchaseDate = null,
    DateTime? NextMaintenanceDue = null,
    DateTime? WarrantyExpiryDate = null,
    string? Notes = null);

public record UpdateEquipmentStatusRequest(string Status, string? Notes = null);

public record MaintenanceRequestResponse(
    Guid Id,
    string Title,
    string Description,
    string RequestType,
    string Priority,
    string Status,
    Guid? RoomId,
    string? RoomName,
    Guid? EquipmentId,
    string? EquipmentName,
    Guid RequestedByUserId,
    string RequestedByUsername,
    Guid? AssignedToUserId,
    string? AssignedToUsername,
    DateTime RequestedAtUtc,
    DateTime? ScheduledDate,
    DateTime? ResolvedAtUtc,
    string? ResolutionNotes);

public record CreateMaintenanceRequestRequest(
    string Title,
    string Description,
    Guid RequestedByUserId,
    string RequestType = "Corrective",
    string Priority = "Medium",
    Guid? RoomId = null,
    Guid? EquipmentId = null,
    DateTime? ScheduledDate = null);

public record AssignMaintenanceRequestRequest(Guid AssignedToUserId, DateTime? ScheduledDate = null);

public record ResolveMaintenanceRequestRequest(string ResolutionNotes);
