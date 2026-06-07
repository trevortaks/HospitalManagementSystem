using HospitalMS.Business.Models;
using HospitalMS.Business.Services;
using HospitalMS.Common.Auth;
using HospitalMS.Common.Constants;
using Microsoft.AspNetCore.Mvc;

namespace HospitalMS.API.Controllers;

[ApiController]
[Route("api/prescriptions")]
[RoleBasedAuth(UserRoles.Admin, UserRoles.Doctor, UserRoles.Pharmacist)]
public sealed class PrescriptionsController(IPrescriptionService prescriptionService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? patientId,
        [FromQuery] Guid? encounterId,
        [FromQuery] string? status,
        CancellationToken cancellationToken)
    {
        var prescriptions = await prescriptionService.GetAllAsync(patientId, encounterId, status, cancellationToken);
        return Ok(prescriptions);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var prescription = await prescriptionService.GetByIdAsync(id, cancellationToken);
        return prescription is null ? NotFound() : Ok(prescription);
    }

    [HttpPost]
    [RoleBasedAuth(UserRoles.Admin, UserRoles.Doctor)]
    public async Task<IActionResult> Create([FromBody] CreatePrescriptionRequest request, CancellationToken cancellationToken)
    {
        var prescription = await prescriptionService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = prescription.Id }, prescription);
    }

    [HttpPatch("{id:guid}/dispense")]
    [RoleBasedAuth(UserRoles.Admin, UserRoles.Pharmacist)]
    public async Task<IActionResult> Dispense(Guid id, [FromBody] DispensePrescriptionRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var prescription = await prescriptionService.DispenseAsync(id, request, cancellationToken);
            return Ok(prescription);
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

    [HttpPatch("{id:guid}/cancel")]
    [RoleBasedAuth(UserRoles.Admin, UserRoles.Doctor)]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var prescription = await prescriptionService.CancelAsync(id, cancellationToken);
            return Ok(prescription);
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
