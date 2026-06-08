using HospitalMS.Business.Models;
using HospitalMS.Business.Services;
using HospitalMS.Common.Auth;
using HospitalMS.Common.Constants;
using Microsoft.AspNetCore.Mvc;

namespace HospitalMS.API.Controllers;

[ApiController]
[Route("api/insurance")]
[RoleBasedAuth(UserRoles.Admin, UserRoles.AccountsManager)]
public sealed class InsuranceController(IInsuranceService insuranceService) : ControllerBase
{
    // Providers
    [HttpGet("providers")]
    public async Task<IActionResult> GetProviders([FromQuery] bool? activeOnly, CancellationToken cancellationToken)
        => Ok(await insuranceService.GetAllProvidersAsync(activeOnly, cancellationToken));

    [HttpGet("providers/{id:guid}")]
    public async Task<IActionResult> GetProvider(Guid id, CancellationToken cancellationToken)
    {
        var p = await insuranceService.GetProviderByIdAsync(id, cancellationToken);
        return p is null ? NotFound() : Ok(p);
    }

    [HttpPost("providers")]
    [RoleBasedAuth(UserRoles.Admin)]
    public async Task<IActionResult> CreateProvider([FromBody] CreateInsuranceProviderRequest request, CancellationToken cancellationToken)
    {
        var p = await insuranceService.CreateProviderAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetProvider), new { id = p.Id }, p);
    }

    [HttpPatch("providers/{id:guid}/toggle-active")]
    [RoleBasedAuth(UserRoles.Admin)]
    public async Task<IActionResult> ToggleProvider(Guid id, CancellationToken cancellationToken)
    {
        try { return Ok(await insuranceService.ToggleProviderActiveAsync(id, cancellationToken)); }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    // Patient insurance
    [HttpGet("patient/{patientId:guid}")]
    public async Task<IActionResult> GetPatientInsurance(Guid patientId, CancellationToken cancellationToken)
        => Ok(await insuranceService.GetPatientInsuranceAsync(patientId, cancellationToken));

    [HttpPost("patient")]
    public async Task<IActionResult> AddPatientInsurance([FromBody] AddPatientInsuranceRequest request, CancellationToken cancellationToken)
        => Ok(await insuranceService.AddPatientInsuranceAsync(request, cancellationToken));

    [HttpPatch("patient/{id:guid}/set-primary")]
    public async Task<IActionResult> SetPrimary(Guid id, CancellationToken cancellationToken)
    {
        try { return Ok(await insuranceService.SetPrimaryAsync(id, cancellationToken)); }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    [HttpDelete("patient/{id:guid}")]
    public async Task<IActionResult> DeletePatientInsurance(Guid id, CancellationToken cancellationToken)
    {
        try { await insuranceService.DeletePatientInsuranceAsync(id, cancellationToken); return NoContent(); }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    // Claims
    [HttpGet("claims/{invoiceId:guid}")]
    public async Task<IActionResult> GetClaims(Guid invoiceId, CancellationToken cancellationToken)
        => Ok(await insuranceService.GetClaimsForInvoiceAsync(invoiceId, cancellationToken));

    [HttpPost("claims")]
    public async Task<IActionResult> CreateClaim([FromBody] CreateInsuranceClaimRequest request, CancellationToken cancellationToken)
    {
        try { return Ok(await insuranceService.CreateClaimAsync(request, cancellationToken)); }
        catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
    }

    [HttpPatch("claims/{id:guid}/status")]
    public async Task<IActionResult> UpdateClaimStatus(Guid id, [FromBody] UpdateClaimStatusRequest request, CancellationToken cancellationToken)
    {
        try { return Ok(await insuranceService.UpdateClaimStatusAsync(id, request, cancellationToken)); }
        catch (KeyNotFoundException) { return NotFound(); }
    }
}
