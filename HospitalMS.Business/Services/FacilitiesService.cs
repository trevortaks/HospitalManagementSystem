using HospitalMS.Business.Models;
using HospitalMS.Data.Persistence;
using HospitalMS.Data.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace HospitalMS.Business.Services;

public sealed class FacilitiesService(HospitalDbContext dbContext) : IFacilitiesService
{
    // ── Rooms ────────────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<RoomResponse>> GetAllRoomsAsync(bool? activeOnly = null, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Rooms.AsNoTracking()
            .Include(r => r.MaintenanceRequests)
            .AsQueryable();
        if (activeOnly == true) query = query.Where(r => r.IsActive);
        var rooms = await query.OrderBy(r => r.RoomNumber).ToListAsync(cancellationToken);
        return rooms.Select(ToRoomResponse).ToArray();
    }

    public async Task<RoomResponse?> GetRoomByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var room = await dbContext.Rooms.AsNoTracking()
            .Include(r => r.MaintenanceRequests)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
        return room is null ? null : ToRoomResponse(room);
    }

    public async Task<RoomResponse> CreateRoomAsync(CreateRoomRequest request, CancellationToken cancellationToken = default)
    {
        var exists = await dbContext.Rooms.AnyAsync(r => r.RoomNumber == request.RoomNumber, cancellationToken);
        if (exists)
            throw new InvalidOperationException($"Room number '{request.RoomNumber}' is already in use.");

        var room = new Room
        {
            Name            = request.Name,
            RoomNumber      = request.RoomNumber,
            RoomType        = request.RoomType,
            FloorNumber     = request.FloorNumber,
            Building        = request.Building,
            CapacityPersons = request.CapacityPersons,
            Notes           = request.Notes
        };
        dbContext.Rooms.Add(room);
        await dbContext.SaveChangesAsync(cancellationToken);
        return ToRoomResponse(room);
    }

    public async Task<RoomResponse> ToggleRoomActiveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var room = await dbContext.Rooms.FindAsync([id], cancellationToken)
            ?? throw new KeyNotFoundException($"Room '{id}' not found.");
        room.IsActive = !room.IsActive;
        await dbContext.SaveChangesAsync(cancellationToken);
        return ToRoomResponse(room);
    }

    // ── Equipment ────────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<EquipmentResponse>> GetAllEquipmentAsync(string? status = null, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Equipment.AsNoTracking()
            .Include(e => e.LocationRoom)
            .AsQueryable();
        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(e => e.Status == status);
        var items = await query.OrderBy(e => e.Name).ToListAsync(cancellationToken);
        return items.Select(ToEquipmentResponse).ToArray();
    }

    public async Task<EquipmentResponse?> GetEquipmentByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await dbContext.Equipment.AsNoTracking()
            .Include(e => e.LocationRoom)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        return item is null ? null : ToEquipmentResponse(item);
    }

    public async Task<EquipmentResponse> CreateEquipmentAsync(CreateEquipmentRequest request, CancellationToken cancellationToken = default)
    {
        var exists = await dbContext.Equipment.AnyAsync(e => e.Code == request.Code, cancellationToken);
        if (exists)
            throw new InvalidOperationException($"Equipment code '{request.Code}' is already in use.");

        var item = new Equipment
        {
            Name               = request.Name,
            Code               = request.Code,
            EquipmentType      = request.EquipmentType,
            SerialNumber       = request.SerialNumber,
            Manufacturer       = request.Manufacturer,
            Model              = request.Model,
            LocationRoomId     = request.LocationRoomId,
            PurchaseDate       = request.PurchaseDate,
            NextMaintenanceDue = request.NextMaintenanceDue,
            WarrantyExpiryDate = request.WarrantyExpiryDate,
            Notes              = request.Notes
        };
        dbContext.Equipment.Add(item);
        await dbContext.SaveChangesAsync(cancellationToken);

        var saved = await dbContext.Equipment.AsNoTracking()
            .Include(e => e.LocationRoom)
            .FirstAsync(e => e.Id == item.Id, cancellationToken);
        return ToEquipmentResponse(saved);
    }

    public async Task<EquipmentResponse> UpdateEquipmentStatusAsync(Guid id, UpdateEquipmentStatusRequest request, CancellationToken cancellationToken = default)
    {
        var item = await dbContext.Equipment.FindAsync([id], cancellationToken)
            ?? throw new KeyNotFoundException($"Equipment '{id}' not found.");

        item.Status = request.Status;
        if (request.Status == EquipmentStatus.Active)
            item.LastMaintenanceDate = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        var saved = await dbContext.Equipment.AsNoTracking()
            .Include(e => e.LocationRoom)
            .FirstAsync(e => e.Id == id, cancellationToken);
        return ToEquipmentResponse(saved);
    }

    public async Task<EquipmentResponse> ToggleEquipmentActiveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await dbContext.Equipment.FindAsync([id], cancellationToken)
            ?? throw new KeyNotFoundException($"Equipment '{id}' not found.");
        item.IsActive = !item.IsActive;
        await dbContext.SaveChangesAsync(cancellationToken);

        var saved = await dbContext.Equipment.AsNoTracking()
            .Include(e => e.LocationRoom)
            .FirstAsync(e => e.Id == id, cancellationToken);
        return ToEquipmentResponse(saved);
    }

    // ── Maintenance Requests ─────────────────────────────────────────────────

    public async Task<IReadOnlyList<MaintenanceRequestResponse>> GetAllRequestsAsync(string? status = null, CancellationToken cancellationToken = default)
    {
        var query = dbContext.MaintenanceRequests.AsNoTracking()
            .Include(m => m.Room)
            .Include(m => m.Equipment)
            .Include(m => m.RequestedByUser)
            .Include(m => m.AssignedToUser)
            .AsQueryable();
        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(m => m.Status == status);
        var requests = await query.OrderByDescending(m => m.RequestedAtUtc).ToListAsync(cancellationToken);
        return requests.Select(ToRequestResponse).ToArray();
    }

    public async Task<IReadOnlyList<MaintenanceRequestResponse>> GetOpenRequestsAsync(CancellationToken cancellationToken = default)
    {
        var requests = await dbContext.MaintenanceRequests.AsNoTracking()
            .Include(m => m.Room)
            .Include(m => m.Equipment)
            .Include(m => m.RequestedByUser)
            .Include(m => m.AssignedToUser)
            .Where(m => m.Status == MaintenanceRequestStatus.Open
                     || m.Status == MaintenanceRequestStatus.InProgress)
            .OrderByDescending(m => m.RequestedAtUtc)
            .ToListAsync(cancellationToken);
        return requests.Select(ToRequestResponse).ToArray();
    }

    public async Task<MaintenanceRequestResponse?> GetRequestByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var request = await LoadRequestAsync(id, cancellationToken);
        return request is null ? null : ToRequestResponse(request);
    }

    public async Task<MaintenanceRequestResponse> CreateRequestAsync(
        CreateMaintenanceRequestRequest request, CancellationToken cancellationToken = default)
    {
        if (request.RoomId is null && request.EquipmentId is null)
            throw new InvalidOperationException("A maintenance request must reference a room or piece of equipment.");

        var entity = new MaintenanceRequest
        {
            Title               = request.Title,
            Description         = request.Description,
            RequestType         = request.RequestType,
            Priority            = request.Priority,
            RoomId              = request.RoomId,
            EquipmentId         = request.EquipmentId,
            RequestedByUserId   = request.RequestedByUserId,
            ScheduledDate       = request.ScheduledDate
        };
        dbContext.MaintenanceRequests.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return ToRequestResponse(await LoadRequestAsync(entity.Id, cancellationToken)!);
    }

    public async Task<MaintenanceRequestResponse> AssignRequestAsync(
        Guid id, AssignMaintenanceRequestRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.MaintenanceRequests.FindAsync([id], cancellationToken)
            ?? throw new KeyNotFoundException($"Maintenance request '{id}' not found.");
        if (entity.Status is MaintenanceRequestStatus.Resolved or MaintenanceRequestStatus.Closed)
            throw new InvalidOperationException($"Cannot assign a {entity.Status.ToLowerInvariant()} request.");

        entity.AssignedToUserId = request.AssignedToUserId;
        entity.ScheduledDate    = request.ScheduledDate ?? entity.ScheduledDate;
        entity.Status           = MaintenanceRequestStatus.InProgress;
        await dbContext.SaveChangesAsync(cancellationToken);
        return ToRequestResponse(await LoadRequestAsync(id, cancellationToken)!);
    }

    public async Task<MaintenanceRequestResponse> ResolveRequestAsync(
        Guid id, ResolveMaintenanceRequestRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.MaintenanceRequests.FindAsync([id], cancellationToken)
            ?? throw new KeyNotFoundException($"Maintenance request '{id}' not found.");
        if (entity.Status is MaintenanceRequestStatus.Resolved or MaintenanceRequestStatus.Closed)
            throw new InvalidOperationException($"Request is already {entity.Status.ToLowerInvariant()}.");

        entity.Status          = MaintenanceRequestStatus.Resolved;
        entity.ResolutionNotes = request.ResolutionNotes;
        entity.ResolvedAtUtc   = DateTime.UtcNow;

        // If linked to equipment, update its last maintenance date
        if (entity.EquipmentId.HasValue)
        {
            var equipment = await dbContext.Equipment.FindAsync([entity.EquipmentId.Value], cancellationToken);
            if (equipment is not null)
                equipment.LastMaintenanceDate = DateTime.UtcNow;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return ToRequestResponse(await LoadRequestAsync(id, cancellationToken)!);
    }

    public async Task<MaintenanceRequestResponse> CloseRequestAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.MaintenanceRequests.FindAsync([id], cancellationToken)
            ?? throw new KeyNotFoundException($"Maintenance request '{id}' not found.");
        if (entity.Status == MaintenanceRequestStatus.Closed)
            throw new InvalidOperationException("Request is already closed.");

        entity.Status = MaintenanceRequestStatus.Closed;
        await dbContext.SaveChangesAsync(cancellationToken);
        return ToRequestResponse(await LoadRequestAsync(id, cancellationToken)!);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private async Task<MaintenanceRequest?> LoadRequestAsync(Guid id, CancellationToken ct) =>
        await dbContext.MaintenanceRequests.AsNoTracking()
            .Include(m => m.Room)
            .Include(m => m.Equipment)
            .Include(m => m.RequestedByUser)
            .Include(m => m.AssignedToUser)
            .FirstOrDefaultAsync(m => m.Id == id, ct);

    private static RoomResponse ToRoomResponse(Room r) =>
        new(r.Id, r.Name, r.RoomNumber, r.RoomType, r.FloorNumber, r.Building,
            r.CapacityPersons, r.IsActive, r.Notes, r.CreatedAtUtc,
            r.MaintenanceRequests.Count(m => m.Status is MaintenanceRequestStatus.Open
                                          or MaintenanceRequestStatus.InProgress));

    private static EquipmentResponse ToEquipmentResponse(Equipment e) =>
        new(e.Id, e.Name, e.Code, e.EquipmentType, e.SerialNumber, e.Manufacturer,
            e.Model, e.LocationRoomId, e.LocationRoom?.Name, e.Status,
            e.PurchaseDate, e.LastMaintenanceDate, e.NextMaintenanceDue,
            e.WarrantyExpiryDate, e.Notes, e.IsActive, e.CreatedAtUtc);

    private static MaintenanceRequestResponse ToRequestResponse(MaintenanceRequest m) =>
        new(m.Id, m.Title, m.Description, m.RequestType, m.Priority, m.Status,
            m.RoomId, m.Room?.Name, m.EquipmentId, m.Equipment?.Name,
            m.RequestedByUserId, m.RequestedByUser.Username,
            m.AssignedToUserId, m.AssignedToUser?.Username,
            m.RequestedAtUtc, m.ScheduledDate, m.ResolvedAtUtc, m.ResolutionNotes);
}
