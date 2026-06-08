using HospitalMS.Business.Models;
using HospitalMS.Business.Services;
using HospitalMS.Common.Auth;
using HospitalMS.Common.Constants;
using Microsoft.AspNetCore.Mvc;

namespace HospitalMS.API.Controllers;

[ApiController]
[Route("api/inventory/items")]
[RoleBasedAuth(UserRoles.Admin, UserRoles.Pharmacist, UserRoles.Nurse)]
public sealed class InventoryItemsController(IInventoryService inventoryService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? categoryId, [FromQuery] bool? lowStockOnly, CancellationToken cancellationToken)
    {
        var items = await inventoryService.GetAllItemsAsync(categoryId, lowStockOnly, cancellationToken);
        return Ok(items);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var item = await inventoryService.GetItemByIdAsync(id, cancellationToken);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    [RoleBasedAuth(UserRoles.Admin, UserRoles.Pharmacist)]
    public async Task<IActionResult> Create([FromBody] CreateInventoryItemRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var item = await inventoryService.CreateItemAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = item.Id }, item);
        }
        catch (KeyNotFoundException e) { return NotFound(e.Message); }
    }

    [HttpPut("{id:guid}")]
    [RoleBasedAuth(UserRoles.Admin, UserRoles.Pharmacist)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateInventoryItemRequest request, CancellationToken cancellationToken)
    {
        try { return Ok(await inventoryService.UpdateItemAsync(id, request, cancellationToken)); }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    [HttpPatch("{id:guid}/toggle-active")]
    [RoleBasedAuth(UserRoles.Admin, UserRoles.Pharmacist)]
    public async Task<IActionResult> ToggleActive(Guid id, CancellationToken cancellationToken)
    {
        try { return Ok(await inventoryService.ToggleItemActiveAsync(id, cancellationToken)); }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    [HttpGet("{id:guid}/transactions")]
    public async Task<IActionResult> GetTransactions(Guid id, CancellationToken cancellationToken)
    {
        var txs = await inventoryService.GetTransactionsByItemAsync(id, cancellationToken);
        return Ok(txs);
    }

    [HttpPost("stock-in")]
    [RoleBasedAuth(UserRoles.Admin, UserRoles.Pharmacist)]
    public async Task<IActionResult> StockIn([FromBody] StockInRequest request, CancellationToken cancellationToken)
    {
        try { return Ok(await inventoryService.StockInAsync(request, cancellationToken)); }
        catch (KeyNotFoundException e) { return NotFound(e.Message); }
        catch (ArgumentException e) { return BadRequest(e.Message); }
    }

    [HttpPost("stock-out")]
    [RoleBasedAuth(UserRoles.Admin, UserRoles.Pharmacist, UserRoles.Nurse)]
    public async Task<IActionResult> StockOut([FromBody] StockOutRequest request, CancellationToken cancellationToken)
    {
        try { return Ok(await inventoryService.StockOutAsync(request, cancellationToken)); }
        catch (KeyNotFoundException e) { return NotFound(e.Message); }
        catch (InvalidOperationException e) { return Conflict(e.Message); }
        catch (ArgumentException e) { return BadRequest(e.Message); }
    }

    [HttpPost("adjust")]
    [RoleBasedAuth(UserRoles.Admin, UserRoles.Pharmacist)]
    public async Task<IActionResult> Adjust([FromBody] AdjustStockRequest request, CancellationToken cancellationToken)
    {
        try { return Ok(await inventoryService.AdjustStockAsync(request, cancellationToken)); }
        catch (KeyNotFoundException e) { return NotFound(e.Message); }
        catch (ArgumentException e) { return BadRequest(e.Message); }
    }
}
