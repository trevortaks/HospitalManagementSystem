using HospitalMS.Business.Models;
using HospitalMS.Business.Services;
using HospitalMS.Data.Persistence.Entities;

namespace HospitalMS.Tests.UnitTests;

public sealed class FacilitiesServiceTests : IntegrationTestBase
{
    private FacilitiesService _service = null!;

    private Guid _userId;
    private Guid _roomId;
    private Guid _equipmentId;

    protected override async Task SeedAsync(HospitalMS.Data.Persistence.HospitalDbContext context)
    {
        var user      = TestFixtures.CreateUser(role: "Nurse");
        var room      = TestFixtures.CreateRoom(roomNumber: "TEST-01");
        var equipment = TestFixtures.CreateEquipment(locationRoomId: room.Id);

        await context.Users.AddAsync(user);
        await context.Rooms.AddAsync(room);
        await context.Equipment.AddAsync(equipment);
        await context.SaveChangesAsync();

        _userId      = user.Id;
        _roomId      = room.Id;
        _equipmentId = equipment.Id;
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _service = new FacilitiesService(Context);
    }

    // ── Room tests ────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateRoomAsync_ReturnsSavedRoom()
    {
        var request = new CreateRoomRequest("ICU Room 1", "ICU-01", "General", 1, "North Wing", 4);
        var result = await _service.CreateRoomAsync(request);

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal("ICU-01", result.RoomNumber);
        Assert.Equal("North Wing", result.Building);
        Assert.True(result.IsActive);
        Assert.Equal(0, result.OpenRequestCount);
    }

    [Fact]
    public async Task CreateRoomAsync_Throws_WhenRoomNumberDuplicate()
    {
        var request = new CreateRoomRequest("Duplicate Room", "TEST-01", "General");
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateRoomAsync(request));
    }

    [Fact]
    public async Task ToggleRoomActiveAsync_FlipsFlag()
    {
        var result1 = await _service.ToggleRoomActiveAsync(_roomId);
        Assert.False(result1.IsActive);

        var result2 = await _service.ToggleRoomActiveAsync(_roomId);
        Assert.True(result2.IsActive);
    }

    // ── Equipment tests ───────────────────────────────────────────────────────

    [Fact]
    public async Task CreateEquipmentAsync_ReturnsSavedEquipment()
    {
        var request = new CreateEquipmentRequest("MRI Scanner", "MRI-001", "Imaging",
            SerialNumber: "SN-12345", Manufacturer: "Siemens");
        var result = await _service.CreateEquipmentAsync(request);

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal("MRI-001", result.Code);
        Assert.Equal("Active", result.Status);
        Assert.Null(result.LocationRoomId);
    }

    [Fact]
    public async Task UpdateEquipmentStatusAsync_ChangesStatus()
    {
        var request = new UpdateEquipmentStatusRequest(EquipmentStatus.UnderMaintenance);
        var result = await _service.UpdateEquipmentStatusAsync(_equipmentId, request);

        Assert.Equal(EquipmentStatus.UnderMaintenance, result.Status);
    }

    [Fact]
    public async Task UpdateEquipmentStatusAsync_Throws_WhenNotFound()
    {
        var request = new UpdateEquipmentStatusRequest(EquipmentStatus.Active);
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _service.UpdateEquipmentStatusAsync(Guid.NewGuid(), request));
    }

    // ── Maintenance Request tests ─────────────────────────────────────────────

    [Fact]
    public async Task CreateRequestAsync_ReturnsOpenRequest()
    {
        var request = new CreateMaintenanceRequestRequest(
            "Broken AC", "Air conditioner not working in OT-01.",
            _userId, "Corrective", "High", RoomId: _roomId);
        var result = await _service.CreateRequestAsync(request);

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal("Open", result.Status);
        Assert.Equal(_roomId, result.RoomId);
        Assert.Equal("High", result.Priority);
    }

    [Fact]
    public async Task CreateRequestAsync_Throws_WhenNoRoomOrEquipment()
    {
        var request = new CreateMaintenanceRequestRequest(
            "Orphan request", "No target.", _userId);
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateRequestAsync(request));
    }

    [Fact]
    public async Task AssignRequestAsync_SetsInProgress()
    {
        var created = await _service.CreateRequestAsync(
            new CreateMaintenanceRequestRequest("Assign test", "Desc", _userId, RoomId: _roomId));
        var assignee = TestFixtures.CreateUser(role: "Nurse");
        await Context.Users.AddAsync(assignee);
        await Context.SaveChangesAsync();

        var result = await _service.AssignRequestAsync(created.Id,
            new AssignMaintenanceRequestRequest(assignee.Id));

        Assert.Equal("InProgress", result.Status);
        Assert.Equal(assignee.Id, result.AssignedToUserId);
    }

    [Fact]
    public async Task ResolveRequestAsync_SetsResolved()
    {
        var created = await _service.CreateRequestAsync(
            new CreateMaintenanceRequestRequest("Resolve test", "Desc", _userId, EquipmentId: _equipmentId));
        var result = await _service.ResolveRequestAsync(created.Id,
            new ResolveMaintenanceRequestRequest("Fixed the equipment."));

        Assert.Equal("Resolved", result.Status);
        Assert.NotNull(result.ResolvedAtUtc);
        Assert.Equal("Fixed the equipment.", result.ResolutionNotes);
    }

    [Fact]
    public async Task ResolveRequestAsync_Throws_WhenAlreadyResolved()
    {
        var created = await _service.CreateRequestAsync(
            new CreateMaintenanceRequestRequest("Double resolve", "Desc", _userId, RoomId: _roomId));
        await _service.ResolveRequestAsync(created.Id, new ResolveMaintenanceRequestRequest("Done."));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.ResolveRequestAsync(created.Id, new ResolveMaintenanceRequestRequest("Again.")));
    }

    [Fact]
    public async Task GetOpenRequestsAsync_FiltersOpenAndInProgress()
    {
        var r1 = await _service.CreateRequestAsync(
            new CreateMaintenanceRequestRequest("Open one", "Desc", _userId, RoomId: _roomId));
        var r2 = await _service.CreateRequestAsync(
            new CreateMaintenanceRequestRequest("To resolve", "Desc", _userId, EquipmentId: _equipmentId));
        await _service.ResolveRequestAsync(r2.Id, new ResolveMaintenanceRequestRequest("Done."));

        var open = await _service.GetOpenRequestsAsync();

        Assert.Contains(open, r => r.Id == r1.Id);
        Assert.DoesNotContain(open, r => r.Id == r2.Id);
    }
}
