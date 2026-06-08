using HospitalMS.Business.Models;
using HospitalMS.Business.Services;
using HospitalMS.Common.Auth;
using HospitalMS.Common.Constants;
using Microsoft.AspNetCore.Mvc;

namespace HospitalMS.API.Controllers;

[ApiController]
[Route("api/beds")]
[RoleBasedAuth(UserRoles.Admin, UserRoles.Nurse, UserRoles.Doctor, UserRoles.Receptionist)]
public sealed class BedsController(IBedManagementService bedService) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var bed = await bedService.GetBedByIdAsync(id, cancellationToken);
        return bed is null ? NotFound() : Ok(bed);
    }

    [HttpPatch("{id:guid}/status")]
    [RoleBasedAuth(UserRoles.Admin, UserRoles.Nurse)]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] string status, CancellationToken cancellationToken)
    {
        try { return Ok(await bedService.UpdateBedStatusAsync(id, status, cancellationToken)); }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    [HttpPatch("{id:guid}/toggle-active")]
    [RoleBasedAuth(UserRoles.Admin)]
    public async Task<IActionResult> ToggleActive(Guid id, CancellationToken cancellationToken)
    {
        try { return Ok(await bedService.ToggleBedActiveAsync(id, cancellationToken)); }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    [HttpGet("availability")]
    public async Task<IActionResult> GetAvailability(CancellationToken cancellationToken)
    {
        var availability = await bedService.GetBedAvailabilityAsync(cancellationToken);
        return Ok(availability);
    }

    [HttpGet("allocations")]
    public async Task<IActionResult> GetActiveAllocations([FromQuery] Guid? wardId, CancellationToken cancellationToken)
    {
        var allocations = await bedService.GetActiveAllocationsAsync(wardId, cancellationToken);
        return Ok(allocations);
    }

    [HttpPost("allocate")]
    [RoleBasedAuth(UserRoles.Admin, UserRoles.Nurse, UserRoles.Doctor)]
    public async Task<IActionResult> Allocate([FromBody] AllocateBedRequest request, CancellationToken cancellationToken)
    {
        try { return Ok(await bedService.AllocateBedAsync(request, cancellationToken)); }
        catch (KeyNotFoundException e) { return NotFound(e.Message); }
        catch (InvalidOperationException e) { return Conflict(e.Message); }
    }

    [HttpPost("discharge")]
    [RoleBasedAuth(UserRoles.Admin, UserRoles.Nurse, UserRoles.Doctor)]
    public async Task<IActionResult> Discharge([FromBody] DischargePatientRequest request, CancellationToken cancellationToken)
    {
        try { return Ok(await bedService.DischargePatientAsync(request, cancellationToken)); }
        catch (KeyNotFoundException e) { return NotFound(e.Message); }
        catch (InvalidOperationException e) { return Conflict(e.Message); }
    }
}
