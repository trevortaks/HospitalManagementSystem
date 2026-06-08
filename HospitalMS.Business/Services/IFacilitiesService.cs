using HospitalMS.Business.Models;

namespace HospitalMS.Business.Services;

public interface IFacilitiesService
{
    // Rooms
    Task<IReadOnlyList<RoomResponse>> GetAllRoomsAsync(bool? activeOnly = null, CancellationToken cancellationToken = default);
    Task<RoomResponse?> GetRoomByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<RoomResponse> CreateRoomAsync(CreateRoomRequest request, CancellationToken cancellationToken = default);
    Task<RoomResponse> ToggleRoomActiveAsync(Guid id, CancellationToken cancellationToken = default);

    // Equipment
    Task<IReadOnlyList<EquipmentResponse>> GetAllEquipmentAsync(string? status = null, CancellationToken cancellationToken = default);
    Task<EquipmentResponse?> GetEquipmentByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<EquipmentResponse> CreateEquipmentAsync(CreateEquipmentRequest request, CancellationToken cancellationToken = default);
    Task<EquipmentResponse> UpdateEquipmentStatusAsync(Guid id, UpdateEquipmentStatusRequest request, CancellationToken cancellationToken = default);
    Task<EquipmentResponse> ToggleEquipmentActiveAsync(Guid id, CancellationToken cancellationToken = default);

    // Maintenance Requests
    Task<IReadOnlyList<MaintenanceRequestResponse>> GetAllRequestsAsync(string? status = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MaintenanceRequestResponse>> GetOpenRequestsAsync(CancellationToken cancellationToken = default);
    Task<MaintenanceRequestResponse?> GetRequestByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<MaintenanceRequestResponse> CreateRequestAsync(CreateMaintenanceRequestRequest request, CancellationToken cancellationToken = default);
    Task<MaintenanceRequestResponse> AssignRequestAsync(Guid id, AssignMaintenanceRequestRequest request, CancellationToken cancellationToken = default);
    Task<MaintenanceRequestResponse> ResolveRequestAsync(Guid id, ResolveMaintenanceRequestRequest request, CancellationToken cancellationToken = default);
    Task<MaintenanceRequestResponse> CloseRequestAsync(Guid id, CancellationToken cancellationToken = default);
}
