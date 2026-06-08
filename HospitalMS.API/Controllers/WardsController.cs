using HospitalMS.Business.Models;
using HospitalMS.Business.Services;
using HospitalMS.Common.Auth;
using HospitalMS.Common.Constants;
using Microsoft.AspNetCore.Mvc;

namespace HospitalMS.API.Controllers;

[ApiController]
[Route("api/wards")]
[RoleBasedAuth(UserRoles.Admin, UserRoles.Nurse, UserRoles.Doctor, UserRoles.Receptionist)]
public sealed class WardsController(IBedManagementService bedService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool? activeOnly, CancellationToken cancellationToken)
    {
        var wards = await bedService.GetAllWardsAsync(activeOnly, cancellationToken);
        return Ok(wards);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var ward = await bedService.GetWardByIdAsync(id, cancellationToken);
        return ward is null ? NotFound() : Ok(ward);
    }

    [HttpPost]
    [RoleBasedAuth(UserRoles.Admin)]
    public async Task<IActionResult> Create([FromBody] CreateWardRequest request, CancellationToken cancellationToken)
    {
        var ward = await bedService.CreateWardAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = ward.Id }, ward);
    }

    [HttpPut("{id:guid}")]
    [RoleBasedAuth(UserRoles.Admin)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateWardRequest request, CancellationToken cancellationToken)
    {
        try { return Ok(await bedService.UpdateWardAsync(id, request, cancellationToken)); }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    [HttpPatch("{id:guid}/toggle-active")]
    [RoleBasedAuth(UserRoles.Admin)]
    public async Task<IActionResult> ToggleActive(Guid id, CancellationToken cancellationToken)
    {
        try { return Ok(await bedService.ToggleWardActiveAsync(id, cancellationToken)); }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    [HttpGet("{wardId:guid}/beds")]
    public async Task<IActionResult> GetBeds(Guid wardId, CancellationToken cancellationToken)
    {
        var beds = await bedService.GetBedsByWardAsync(wardId, cancellationToken);
        return Ok(beds);
    }

    [HttpPost("{wardId:guid}/beds")]
    [RoleBasedAuth(UserRoles.Admin)]
    public async Task<IActionResult> CreateBed(Guid wardId, [FromBody] CreateBedRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var bed = await bedService.CreateBedAsync(request with { WardId = wardId }, cancellationToken);
            return Ok(bed);
        }
        catch (KeyNotFoundException) { return NotFound(); }
    }
}
