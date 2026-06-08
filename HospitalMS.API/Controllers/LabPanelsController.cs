using HospitalMS.Business.Models;
using HospitalMS.Business.Services;
using HospitalMS.Common.Auth;
using HospitalMS.Common.Constants;
using Microsoft.AspNetCore.Mvc;

namespace HospitalMS.API.Controllers;

[ApiController]
[Route("api/lab-panels")]
[RoleBasedAuth(UserRoles.Admin, UserRoles.Doctor, UserRoles.Nurse, UserRoles.LabTechnician, UserRoles.Radiologist)]
public sealed class LabPanelsController(ILabService labService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool? activeOnly, CancellationToken cancellationToken)
    {
        var panels = await labService.GetAllPanelsAsync(activeOnly, cancellationToken);
        return Ok(panels);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var panel = await labService.GetPanelByIdAsync(id, cancellationToken);
        return panel is null ? NotFound() : Ok(panel);
    }

    [HttpPost]
    [RoleBasedAuth(UserRoles.Admin)]
    public async Task<IActionResult> Create([FromBody] CreateLabOrderPanelRequest request, CancellationToken cancellationToken)
    {
        var panel = await labService.CreatePanelAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = panel.Id }, panel);
    }

    [HttpPatch("{id:guid}/toggle-active")]
    [RoleBasedAuth(UserRoles.Admin)]
    public async Task<IActionResult> ToggleActive(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var panel = await labService.TogglePanelActiveAsync(id, cancellationToken);
            return Ok(panel);
        }
        catch (KeyNotFoundException) { return NotFound(); }
    }
}
