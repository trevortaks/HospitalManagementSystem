using HospitalMS.Business.Models;
using HospitalMS.Business.Services;
using HospitalMS.Common.Auth;
using HospitalMS.Common.Constants;
using Microsoft.AspNetCore.Mvc;

namespace HospitalMS.API.Controllers;

[ApiController]
[Route("api/appointments")]
[RoleBasedAuth(UserRoles.Admin, UserRoles.Doctor, UserRoles.Nurse, UserRoles.Receptionist)]
public sealed class AppointmentsController(IAppointmentService appointmentService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? patientId,
        [FromQuery] Guid? doctorUserId,
        [FromQuery] string? status,
        CancellationToken cancellationToken)
    {
        var appointments = await appointmentService.GetAllAsync(patientId, doctorUserId, status, cancellationToken);
        return Ok(appointments);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var appointment = await appointmentService.GetByIdAsync(id, cancellationToken);
        return appointment is null ? NotFound() : Ok(appointment);
    }

    [HttpPost]
    [RoleBasedAuth(UserRoles.Admin, UserRoles.Doctor, UserRoles.Receptionist)]
    public async Task<IActionResult> Create([FromBody] CreateAppointmentRequest request, CancellationToken cancellationToken)
    {
        var appointment = await appointmentService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = appointment.Id }, appointment);
    }

    [HttpPut("{id:guid}")]
    [RoleBasedAuth(UserRoles.Admin, UserRoles.Doctor, UserRoles.Receptionist)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAppointmentRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var appointment = await appointmentService.UpdateAsync(id, request, cancellationToken);
            return Ok(appointment);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPatch("{id:guid}/cancel")]
    [RoleBasedAuth(UserRoles.Admin, UserRoles.Doctor, UserRoles.Receptionist)]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelAppointmentRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var appointment = await appointmentService.CancelAsync(id, request, cancellationToken);
            return Ok(appointment);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPatch("{id:guid}/status")]
    [RoleBasedAuth(UserRoles.Admin, UserRoles.Doctor)]
    public async Task<IActionResult> PatchStatus(Guid id, [FromBody] PatchAppointmentStatusRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var appointment = await appointmentService.PatchStatusAsync(id, request, cancellationToken);
            return Ok(appointment);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpDelete("{id:guid}")]
    [RoleBasedAuth(UserRoles.Admin)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await appointmentService.DeleteAsync(id, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpGet("{id:guid}/vitals")]
    public async Task<IActionResult> GetVitals(Guid id, CancellationToken cancellationToken)
    {
        var vitals = await appointmentService.GetVitalsAsync(id, cancellationToken);
        return vitals is null ? NotFound() : Ok(vitals);
    }

    [HttpPost("{id:guid}/vitals")]
    [RoleBasedAuth(UserRoles.Admin, UserRoles.Nurse, UserRoles.Doctor)]
    public async Task<IActionResult> RecordVitals(Guid id, [FromBody] RecordAppointmentVitalsRequest request, CancellationToken cancellationToken)
    {
        var userId = User.FindFirst("sub")?.Value;
        if (!Guid.TryParse(userId, out var recordedByUserId))
            return Unauthorized();

        try
        {
            var vitals = await appointmentService.RecordVitalsAsync(id, recordedByUserId, request, cancellationToken);
            return Ok(vitals);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}
