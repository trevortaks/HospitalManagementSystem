using HospitalMS.Business.Models;
using HospitalMS.Business.Services;
using HospitalMS.Common.Auth;
using HospitalMS.Common.Constants;
using Microsoft.AspNetCore.Mvc;

namespace HospitalMS.API.Controllers;

[ApiController]
[Route("api/purchase-orders")]
[RoleBasedAuth(UserRoles.Admin, UserRoles.Pharmacist, UserRoles.AccountsManager)]
public sealed class PurchaseOrdersController(ISupplyChainService supplyChainService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? status, CancellationToken cancellationToken)
    {
        var orders = await supplyChainService.GetAllPurchaseOrdersAsync(status, cancellationToken);
        return Ok(orders);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var po = await supplyChainService.GetPurchaseOrderByIdAsync(id, cancellationToken);
        return po is null ? NotFound() : Ok(po);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePurchaseOrderRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var po = await supplyChainService.CreatePurchaseOrderAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = po.Id }, po);
        }
        catch (KeyNotFoundException e) { return NotFound(e.Message); }
        catch (InvalidOperationException e) { return BadRequest(e.Message); }
    }

    [HttpPatch("{id:guid}/submit")]
    public async Task<IActionResult> Submit(Guid id, CancellationToken cancellationToken)
    {
        try { return Ok(await supplyChainService.SubmitPurchaseOrderAsync(id, cancellationToken)); }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (InvalidOperationException e) { return Conflict(e.Message); }
    }

    [HttpPatch("{id:guid}/approve")]
    [RoleBasedAuth(UserRoles.Admin)]
    public async Task<IActionResult> Approve(Guid id, CancellationToken cancellationToken)
    {
        try { return Ok(await supplyChainService.ApprovePurchaseOrderAsync(id, cancellationToken)); }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (InvalidOperationException e) { return Conflict(e.Message); }
    }

    [HttpPatch("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
    {
        try { return Ok(await supplyChainService.CancelPurchaseOrderAsync(id, cancellationToken)); }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (InvalidOperationException e) { return Conflict(e.Message); }
    }

    [HttpPost("{id:guid}/receive")]
    [RoleBasedAuth(UserRoles.Admin, UserRoles.Pharmacist)]
    public async Task<IActionResult> ReceiveGoods(Guid id, [FromBody] ReceiveGoodsRequest request, CancellationToken cancellationToken)
    {
        try { return Ok(await supplyChainService.ReceiveGoodsAsync(request with { PurchaseOrderId = id }, cancellationToken)); }
        catch (KeyNotFoundException e) { return NotFound(e.Message); }
        catch (InvalidOperationException e) { return Conflict(e.Message); }
    }

    [HttpGet("{id:guid}/receipts")]
    public async Task<IActionResult> GetReceipts(Guid id, CancellationToken cancellationToken)
    {
        var receipts = await supplyChainService.GetReceiptsByPurchaseOrderAsync(id, cancellationToken);
        return Ok(receipts);
    }
}
