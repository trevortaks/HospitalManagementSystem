using HospitalMS.Business.Models;
using HospitalMS.Business.Services;
using HospitalMS.Common.Auth;
using HospitalMS.Common.Constants;
using Microsoft.AspNetCore.Mvc;

namespace HospitalMS.API.Controllers;

[ApiController]
[Route("api/quality")]
[RoleBasedAuth(UserRoles.Admin, UserRoles.Doctor, UserRoles.Nurse)]
public sealed class QualityController(IQualityService qualityService) : ControllerBase
{
    [HttpGet("summary")]
    public async Task<IActionResult> Summary(CancellationToken ct)
        => Ok(await qualityService.GetSummaryAsync(ct));

    // ── Incidents ────────────────────────────────────────────────────────────

    [HttpGet("incidents")]
    public async Task<IActionResult> GetIncidents(
        [FromQuery] string? status, [FromQuery] string? severity, CancellationToken ct)
        => Ok(await qualityService.GetIncidentsAsync(status, severity, ct));

    [HttpGet("incidents/{id:guid}")]
    public async Task<IActionResult> GetIncident(Guid id, CancellationToken ct)
    {
        try { return Ok(await qualityService.GetIncidentByIdAsync(id, ct)); }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    [HttpPost("incidents")]
    public async Task<IActionResult> CreateIncident(
        [FromBody] CreateQualityIncidentRequest request, CancellationToken ct)
    {
        var result = await qualityService.CreateIncidentAsync(request, ct);
        return CreatedAtAction(nameof(GetIncident), new { id = result.Id }, result);
    }

    [HttpPost("incidents/{id:guid}/assign")]
    public async Task<IActionResult> Assign(
        Guid id, [FromBody] AssignQualityIncidentRequest request, CancellationToken ct)
    {
        try { return Ok(await qualityService.AssignIncidentAsync(id, request, ct)); }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    [HttpPost("incidents/{id:guid}/investigate")]
    public async Task<IActionResult> Investigate(
        Guid id, [FromBody] InvestigateQualityIncidentRequest request, CancellationToken ct)
    {
        try { return Ok(await qualityService.StartInvestigationAsync(id, request, ct)); }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    [HttpPost("incidents/{id:guid}/resolve")]
    public async Task<IActionResult> Resolve(
        Guid id, [FromBody] ResolveQualityIncidentRequest request, CancellationToken ct)
    {
        try { return Ok(await qualityService.ResolveIncidentAsync(id, request, ct)); }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
    }

    [HttpPost("incidents/{id:guid}/close")]
    public async Task<IActionResult> Close(Guid id, CancellationToken ct)
    {
        try { return Ok(await qualityService.CloseIncidentAsync(id, ct)); }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
    }

    // ── Patient Feedback ────────────────────────────────────────────────────

    [HttpGet("feedback")]
    public async Task<IActionResult> GetFeedback(
        [FromQuery] Guid? patientId, CancellationToken ct)
        => Ok(await qualityService.GetFeedbackAsync(patientId, ct));

    [HttpPost("feedback")]
    public async Task<IActionResult> SubmitFeedback(
        [FromBody] CreatePatientFeedbackRequest request, CancellationToken ct)
    {
        try { return Ok(await qualityService.SubmitFeedbackAsync(request, ct)); }
        catch (ArgumentException ex) { return BadRequest(new { error = ex.Message }); }
    }
}
