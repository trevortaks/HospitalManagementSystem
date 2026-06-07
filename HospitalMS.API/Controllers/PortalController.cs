using HospitalMS.Business.Models;
using HospitalMS.Business.Services;
using HospitalMS.Common.Auth;
using HospitalMS.Common.Constants;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HospitalMS.API.Controllers;

[ApiController]
[Route("api/portal")]
[RoleBasedAuth(UserRoles.Patient, UserRoles.Admin)]
public sealed class PortalController(IPortalService portalService) : ControllerBase
{
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var dashboard = await portalService.GetDashboardAsync(userId.Value, cancellationToken);
        return dashboard is null ? NotFound(new { message = "No patient record linked to this account." }) : Ok(dashboard);
    }

    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var profile = await portalService.GetProfileAsync(userId.Value, cancellationToken);
        return profile is null ? NotFound(new { message = "No patient record linked to this account." }) : Ok(profile);
    }

    [HttpPatch("profile")]
    public async Task<IActionResult> UpdateProfile(
        [FromBody] UpdatePortalProfileRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var profile = await portalService.UpdateProfileAsync(userId.Value, request, cancellationToken);
        return profile is null ? NotFound(new { message = "No patient record linked to this account." }) : Ok(profile);
    }

    [HttpGet("appointments")]
    public async Task<IActionResult> GetAppointments(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var appointments = await portalService.GetAppointmentsAsync(userId.Value, cancellationToken);
        return Ok(appointments);
    }

    [HttpGet("encounters")]
    public async Task<IActionResult> GetEncounters(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var encounters = await portalService.GetEncountersAsync(userId.Value, cancellationToken);
        return Ok(encounters);
    }

    [HttpGet("prescriptions")]
    public async Task<IActionResult> GetPrescriptions(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var prescriptions = await portalService.GetPrescriptionsAsync(userId.Value, cancellationToken);
        return Ok(prescriptions);
    }

    [HttpPost("session")]
    public async Task<IActionResult> LogSession(
        [FromBody] LogPortalSessionRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId() ?? Guid.Empty;
        // Always resolve userId from the JWT, not the caller
        await portalService.LogSessionAsync(request with { UserId = userId }, cancellationToken);
        return NoContent();
    }

    private Guid? GetUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(claim, out var id) ? id : null;
    }
}
