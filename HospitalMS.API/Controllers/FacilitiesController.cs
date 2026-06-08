using HospitalMS.Business.Models;
using HospitalMS.Business.Services;
using HospitalMS.Common.Auth;
using HospitalMS.Common.Constants;
using Microsoft.AspNetCore.Mvc;

namespace HospitalMS.API.Controllers;

[ApiController]
[Route("api/facilities")]
[RoleBasedAuth(UserRoles.Admin, UserRoles.Nurse, UserRoles.Doctor)]
public sealed class FacilitiesController(IFacilitiesService facilitiesService) : ControllerBase
{
    // ── Rooms ─────────────────────────────────────────────────────────────────

    [HttpGet("rooms")]
    public async Task<IActionResult> GetRooms([FromQuery] bool? activeOnly, CancellationToken ct)
        => Ok(await facilitiesService.GetAllRoomsAsync(activeOnly, ct));

    [HttpGet("rooms/{id:guid}")]
    public async Task<IActionResult> GetRoom(Guid id, CancellationToken ct)
    {
        var room = await facilitiesService.GetRoomByIdAsync(id, ct);
        return room is null ? NotFound() : Ok(room);
    }

    [HttpPost("rooms")]
    [RoleBasedAuth(UserRoles.Admin)]
    public async Task<IActionResult> CreateRoom([FromBody] CreateRoomRequest request, CancellationToken ct)
    {
        try
        {
            var room = await facilitiesService.CreateRoomAsync(request, ct);
            return CreatedAtAction(nameof(GetRoom), new { id = room.Id }, room);
        }
        catch (InvalidOperationException ex) { return Conflict(new { error = ex.Message }); }
    }

    [HttpPatch("rooms/{id:guid}/toggle-active")]
    [RoleBasedAuth(UserRoles.Admin)]
    public async Task<IActionResult> ToggleRoomActive(Guid id, CancellationToken ct)
    {
        try { return Ok(await facilitiesService.ToggleRoomActiveAsync(id, ct)); }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    // ── Equipment ─────────────────────────────────────────────────────────────

    [HttpGet("equipment")]
    public async Task<IActionResult> GetEquipment([FromQuery] string? status, CancellationToken ct)
        => Ok(await facilitiesService.GetAllEquipmentAsync(status, ct));

    [HttpGet("equipment/{id:guid}")]
    public async Task<IActionResult> GetEquipmentById(Guid id, CancellationToken ct)
    {
        var item = await facilitiesService.GetEquipmentByIdAsync(id, ct);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost("equipment")]
    [RoleBasedAuth(UserRoles.Admin)]
    public async Task<IActionResult> CreateEquipment([FromBody] CreateEquipmentRequest request, CancellationToken ct)
    {
        try
        {
            var item = await facilitiesService.CreateEquipmentAsync(request, ct);
            return CreatedAtAction(nameof(GetEquipmentById), new { id = item.Id }, item);
        }
        catch (InvalidOperationException ex) { return Conflict(new { error = ex.Message }); }
    }

    [HttpPatch("equipment/{id:guid}/status")]
    public async Task<IActionResult> UpdateEquipmentStatus(Guid id, [FromBody] UpdateEquipmentStatusRequest request, CancellationToken ct)
    {
        try { return Ok(await facilitiesService.UpdateEquipmentStatusAsync(id, request, ct)); }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    [HttpPatch("equipment/{id:guid}/toggle-active")]
    [RoleBasedAuth(UserRoles.Admin)]
    public async Task<IActionResult> ToggleEquipmentActive(Guid id, CancellationToken ct)
    {
        try { return Ok(await facilitiesService.ToggleEquipmentActiveAsync(id, ct)); }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    // ── Maintenance Requests ──────────────────────────────────────────────────

    [HttpGet("maintenance-requests")]
    public async Task<IActionResult> GetMaintenanceRequests([FromQuery] string? status, CancellationToken ct)
        => Ok(await facilitiesService.GetAllRequestsAsync(status, ct));

    [HttpGet("maintenance-requests/open")]
    public async Task<IActionResult> GetOpenRequests(CancellationToken ct)
        => Ok(await facilitiesService.GetOpenRequestsAsync(ct));

    [HttpGet("maintenance-requests/{id:guid}")]
    public async Task<IActionResult> GetMaintenanceRequest(Guid id, CancellationToken ct)
    {
        var req = await facilitiesService.GetRequestByIdAsync(id, ct);
        return req is null ? NotFound() : Ok(req);
    }

    [HttpPost("maintenance-requests")]
    public async Task<IActionResult> CreateMaintenanceRequest([FromBody] CreateMaintenanceRequestRequest request, CancellationToken ct)
    {
        try
        {
            var result = await facilitiesService.CreateRequestAsync(request, ct);
            return CreatedAtAction(nameof(GetMaintenanceRequest), new { id = result.Id }, result);
        }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
    }

    [HttpPatch("maintenance-requests/{id:guid}/assign")]
    public async Task<IActionResult> AssignRequest(Guid id, [FromBody] AssignMaintenanceRequestRequest request, CancellationToken ct)
    {
        try { return Ok(await facilitiesService.AssignRequestAsync(id, request, ct)); }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (InvalidOperationException ex) { return Conflict(new { error = ex.Message }); }
    }

    [HttpPatch("maintenance-requests/{id:guid}/resolve")]
    public async Task<IActionResult> ResolveRequest(Guid id, [FromBody] ResolveMaintenanceRequestRequest request, CancellationToken ct)
    {
        try { return Ok(await facilitiesService.ResolveRequestAsync(id, request, ct)); }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (InvalidOperationException ex) { return Conflict(new { error = ex.Message }); }
    }

    [HttpPatch("maintenance-requests/{id:guid}/close")]
    [RoleBasedAuth(UserRoles.Admin)]
    public async Task<IActionResult> CloseRequest(Guid id, CancellationToken ct)
    {
        try { return Ok(await facilitiesService.CloseRequestAsync(id, ct)); }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (InvalidOperationException ex) { return Conflict(new { error = ex.Message }); }
    }
}
