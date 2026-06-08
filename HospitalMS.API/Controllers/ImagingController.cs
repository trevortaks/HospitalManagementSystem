using HospitalMS.Business.Models;
using HospitalMS.Business.Services;
using HospitalMS.Common.Auth;
using HospitalMS.Common.Constants;
using Microsoft.AspNetCore.Mvc;

namespace HospitalMS.API.Controllers;

[ApiController]
[Route("api/imaging-requests")]
[RoleBasedAuth(UserRoles.Admin, UserRoles.Doctor, UserRoles.Nurse, UserRoles.Radiologist)]
public sealed class ImagingController(IImagingService imagingService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? patientId,
        [FromQuery] Guid? encounterId,
        [FromQuery] string? status,
        CancellationToken cancellationToken)
    {
        var requests = await imagingService.GetAllRequestsAsync(patientId, encounterId, status, cancellationToken);
        return Ok(requests);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var request = await imagingService.GetRequestByIdAsync(id, cancellationToken);
        return request is null ? NotFound() : Ok(request);
    }

    [HttpPost]
    [RoleBasedAuth(UserRoles.Admin, UserRoles.Doctor)]
    public async Task<IActionResult> Create([FromBody] CreateImagingRequestRequest request, CancellationToken cancellationToken)
    {
        var result = await imagingService.CreateRequestAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPatch("{id:guid}/status")]
    [RoleBasedAuth(UserRoles.Admin, UserRoles.Radiologist)]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] string status, CancellationToken cancellationToken)
    {
        try
        {
            var result = await imagingService.UpdateStatusAsync(id, status, cancellationToken);
            return Ok(result);
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
            var result = await imagingService.CancelRequestAsync(id, cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
    }

    [HttpPost("{id:guid}/report")]
    [RoleBasedAuth(UserRoles.Admin, UserRoles.Radiologist)]
    public async Task<IActionResult> CreateReport(Guid id, [FromBody] CreateImagingReportRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await imagingService.CreateReportAsync(id, request, cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
    }
}
