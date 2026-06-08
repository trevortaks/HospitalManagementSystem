using HospitalMS.Business.Models;
using HospitalMS.Business.Services;
using HospitalMS.Common.Auth;
using HospitalMS.Common.Constants;
using Microsoft.AspNetCore.Mvc;

namespace HospitalMS.API.Controllers;

[ApiController]
[Route("api/invoices")]
[RoleBasedAuth(UserRoles.Admin, UserRoles.AccountsManager, UserRoles.Doctor, UserRoles.Receptionist)]
public sealed class InvoicesController(IBillingService billingService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? patientId, [FromQuery] string? status, CancellationToken cancellationToken)
    {
        var invoices = await billingService.GetAllInvoicesAsync(patientId, status, cancellationToken);
        return Ok(invoices);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var invoice = await billingService.GetInvoiceByIdAsync(id, cancellationToken);
        return invoice is null ? NotFound() : Ok(invoice);
    }

    [HttpPost]
    [RoleBasedAuth(UserRoles.Admin, UserRoles.AccountsManager, UserRoles.Receptionist)]
    public async Task<IActionResult> Create([FromBody] CreateInvoiceRequest request, CancellationToken cancellationToken)
    {
        var invoice = await billingService.CreateInvoiceAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = invoice.Id }, invoice);
    }

    [HttpPut("{id:guid}")]
    [RoleBasedAuth(UserRoles.Admin, UserRoles.AccountsManager)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateInvoiceRequest request, CancellationToken cancellationToken)
    {
        try { return Ok(await billingService.UpdateInvoiceAsync(id, request, cancellationToken)); }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
    }

    [HttpPost("{id:guid}/line-items")]
    [RoleBasedAuth(UserRoles.Admin, UserRoles.AccountsManager)]
    public async Task<IActionResult> AddLineItem(Guid id, [FromBody] AddLineItemRequest request, CancellationToken cancellationToken)
    {
        try { return Ok(await billingService.AddLineItemAsync(id, request, cancellationToken)); }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
    }

    [HttpDelete("{id:guid}/line-items/{lineItemId:guid}")]
    [RoleBasedAuth(UserRoles.Admin, UserRoles.AccountsManager)]
    public async Task<IActionResult> RemoveLineItem(Guid id, Guid lineItemId, CancellationToken cancellationToken)
    {
        try { return Ok(await billingService.RemoveLineItemAsync(id, lineItemId, cancellationToken)); }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
    }

    [HttpPatch("{id:guid}/issue")]
    [RoleBasedAuth(UserRoles.Admin, UserRoles.AccountsManager)]
    public async Task<IActionResult> Issue(Guid id, CancellationToken cancellationToken)
    {
        try { return Ok(await billingService.IssueInvoiceAsync(id, cancellationToken)); }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
    }

    [HttpPatch("{id:guid}/void")]
    [RoleBasedAuth(UserRoles.Admin, UserRoles.AccountsManager)]
    public async Task<IActionResult> Void(Guid id, CancellationToken cancellationToken)
    {
        try { return Ok(await billingService.VoidInvoiceAsync(id, cancellationToken)); }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
    }

    [HttpPost("{id:guid}/payments")]
    [RoleBasedAuth(UserRoles.Admin, UserRoles.AccountsManager)]
    public async Task<IActionResult> RecordPayment(Guid id, [FromBody] RecordPaymentRequest request, CancellationToken cancellationToken)
    {
        try { return Ok(await billingService.RecordPaymentAsync(request with { InvoiceId = id }, cancellationToken)); }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return BadRequest(ex.Message); }
    }
}
