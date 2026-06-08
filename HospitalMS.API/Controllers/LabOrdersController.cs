using HospitalMS.Business.Models;
using HospitalMS.Business.Services;
using HospitalMS.Common.Auth;
using HospitalMS.Common.Constants;
using Microsoft.AspNetCore.Mvc;

namespace HospitalMS.API.Controllers;

[ApiController]
[Route("api/lab-orders")]
[RoleBasedAuth(UserRoles.Admin, UserRoles.Doctor, UserRoles.Nurse, UserRoles.LabTechnician)]
public sealed class LabOrdersController(ILabService labService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? patientId,
        [FromQuery] Guid? encounterId,
        [FromQuery] string? status,
        CancellationToken cancellationToken)
    {
        var orders = await labService.GetAllOrdersAsync(patientId, encounterId, status, cancellationToken);
        return Ok(orders);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var order = await labService.GetOrderByIdAsync(id, cancellationToken);
        return order is null ? NotFound() : Ok(order);
    }

    [HttpPost]
    [RoleBasedAuth(UserRoles.Admin, UserRoles.Doctor)]
    public async Task<IActionResult> Create([FromBody] CreateLabOrderRequest request, CancellationToken cancellationToken)
    {
        var order = await labService.CreateOrderAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
    }

    [HttpPatch("{id:guid}/collect")]
    [RoleBasedAuth(UserRoles.Admin, UserRoles.LabTechnician, UserRoles.Nurse)]
    public async Task<IActionResult> Collect(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var order = await labService.CollectOrderAsync(id, cancellationToken);
            return Ok(order);
        }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
    }

    [HttpPatch("{id:guid}/cancel")]
    [RoleBasedAuth(UserRoles.Admin, UserRoles.Doctor)]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var order = await labService.CancelOrderAsync(id, cancellationToken);
            return Ok(order);
        }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
    }

    [HttpPost("{id:guid}/results")]
    [RoleBasedAuth(UserRoles.Admin, UserRoles.LabTechnician, UserRoles.Radiologist)]
    public async Task<IActionResult> AddResult(Guid id, [FromBody] AddLabResultRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var order = await labService.AddResultAsync(id, request, cancellationToken);
            return Ok(order);
        }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
    }
}
