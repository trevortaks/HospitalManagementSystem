using HospitalMS.Business.Models;
using HospitalMS.Business.Services;
using HospitalMS.Common.Auth;
using HospitalMS.Common.Constants;
using Microsoft.AspNetCore.Mvc;

namespace HospitalMS.API.Controllers;

[ApiController]
[Route("api/medications")]
[RoleBasedAuth(UserRoles.Admin, UserRoles.Doctor, UserRoles.Nurse, UserRoles.Pharmacist)]
public sealed class MedicationsController(IMedicationService medicationService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool? activeOnly, CancellationToken cancellationToken)
    {
        var meds = await medicationService.GetAllAsync(activeOnly, cancellationToken);
        return Ok(meds);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var med = await medicationService.GetByIdAsync(id, cancellationToken);
        return med is null ? NotFound() : Ok(med);
    }

    [HttpPost]
    [RoleBasedAuth(UserRoles.Admin, UserRoles.Doctor)]
    public async Task<IActionResult> Create([FromBody] CreateMedicationRequest request, CancellationToken cancellationToken)
    {
        var med = await medicationService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = med.Id }, med);
    }

    [HttpPut("{id:guid}")]
    [RoleBasedAuth(UserRoles.Admin, UserRoles.Doctor)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateMedicationRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var med = await medicationService.UpdateAsync(id, request, cancellationToken);
            return Ok(med);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPatch("{id:guid}/toggle-active")]
    [RoleBasedAuth(UserRoles.Admin)]
    public async Task<IActionResult> ToggleActive(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var med = await medicationService.ToggleActiveAsync(id, cancellationToken);
            return Ok(med);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}
