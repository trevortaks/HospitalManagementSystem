using HospitalMS.Business.Models;
using HospitalMS.Business.Services;
using HospitalMS.Common.Auth;
using HospitalMS.Common.Constants;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HospitalMS.API.Controllers;

[ApiController]
[Route("api/encounters")]
[RoleBasedAuth(UserRoles.Admin, UserRoles.Doctor, UserRoles.Nurse)]
public sealed class EncountersController(IEncounterService encounterService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? patientId,
        [FromQuery] bool? isClosed,
        [FromQuery] Guid? attendingDoctorId,
        CancellationToken cancellationToken)
    {
        var encounters = await encounterService.GetAllAsync(patientId, isClosed, attendingDoctorId, cancellationToken);
        return Ok(encounters);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var encounter = await encounterService.GetByIdAsync(id, cancellationToken);
        return encounter is null ? NotFound() : Ok(encounter);
    }

    [HttpPost]
    [RoleBasedAuth(UserRoles.Admin, UserRoles.Doctor, UserRoles.Nurse)]
    public async Task<IActionResult> Create([FromBody] CreateEncounterRequest request, CancellationToken cancellationToken)
    {
        var encounter = await encounterService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = encounter.Id }, encounter);
    }

    [HttpPut("{id:guid}")]
    [RoleBasedAuth(UserRoles.Admin, UserRoles.Doctor, UserRoles.Nurse)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateEncounterRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var encounter = await encounterService.UpdateAsync(id, request, cancellationToken);
            return Ok(encounter);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPost("{id:guid}/close")]
    [RoleBasedAuth(UserRoles.Admin, UserRoles.Doctor)]
    public async Task<IActionResult> Close(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var encounter = await encounterService.CloseAsync(id, cancellationToken);
            return Ok(encounter);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    // ── Diagnoses sub-resource ────────────────────────────────────────────────

    [HttpGet("{id:guid}/diagnoses")]
    public async Task<IActionResult> GetDiagnoses(Guid id, CancellationToken cancellationToken)
    {
        var encounter = await encounterService.GetByIdAsync(id, cancellationToken);
        if (encounter is null) return NotFound();
        return Ok(encounter.Diagnoses);
    }

    [HttpPost("{id:guid}/diagnoses")]
    [RoleBasedAuth(UserRoles.Admin, UserRoles.Doctor)]
    public async Task<IActionResult> AddDiagnosis(Guid id, [FromBody] AddDiagnosisRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var diagnosis = await encounterService.AddDiagnosisAsync(id, request, cancellationToken);
            return Created($"/api/encounters/{id}/diagnoses/{diagnosis.Id}", diagnosis);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:guid}/diagnoses/{diagnosisId:guid}")]
    [RoleBasedAuth(UserRoles.Admin, UserRoles.Doctor)]
    public async Task<IActionResult> DeleteDiagnosis(Guid id, Guid diagnosisId, CancellationToken cancellationToken)
    {
        try
        {
            await encounterService.DeleteDiagnosisAsync(id, diagnosisId, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    // ── Vitals sub-resource ───────────────────────────────────────────────────

    [HttpGet("{id:guid}/vitals")]
    public async Task<IActionResult> GetVitals(Guid id, CancellationToken cancellationToken)
    {
        var encounter = await encounterService.GetByIdAsync(id, cancellationToken);
        if (encounter is null) return NotFound();
        return Ok(encounter.VitalSigns);
    }

    [HttpPost("{id:guid}/vitals")]
    [RoleBasedAuth(UserRoles.Admin, UserRoles.Doctor, UserRoles.Nurse)]
    public async Task<IActionResult> AddVitals(Guid id, [FromBody] AddVitalsRequest request, CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");

        if (!Guid.TryParse(userIdClaim, out var recordedByUserId))
            return Unauthorized();

        try
        {
            var vitals = await encounterService.AddVitalsAsync(id, recordedByUserId, request, cancellationToken);
            return Created($"/api/encounters/{id}/vitals/{vitals.Id}", vitals);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }
}
