using HospitalMS.Business.Models;
using HospitalMS.Business.Services;
using HospitalMS.Common.Auth;
using HospitalMS.Common.Constants;
using Microsoft.AspNetCore.Mvc;

namespace HospitalMS.API.Controllers;

[ApiController]
[Route("api/charge-items")]
[RoleBasedAuth(UserRoles.Admin, UserRoles.AccountsManager)]
public sealed class ChargeItemsController(IBillingService billingService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool? activeOnly, CancellationToken cancellationToken)
    {
        var items = await billingService.GetAllChargeItemsAsync(activeOnly, cancellationToken);
        return Ok(items);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var item = await billingService.GetChargeItemByIdAsync(id, cancellationToken);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    [RoleBasedAuth(UserRoles.Admin)]
    public async Task<IActionResult> Create([FromBody] CreateChargeItemRequest request, CancellationToken cancellationToken)
    {
        var item = await billingService.CreateChargeItemAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = item.Id }, item);
    }

    [HttpPut("{id:guid}")]
    [RoleBasedAuth(UserRoles.Admin)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateChargeItemRequest request, CancellationToken cancellationToken)
    {
        try { return Ok(await billingService.UpdateChargeItemAsync(id, request, cancellationToken)); }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    [HttpPatch("{id:guid}/toggle-active")]
    [RoleBasedAuth(UserRoles.Admin)]
    public async Task<IActionResult> ToggleActive(Guid id, CancellationToken cancellationToken)
    {
        try { return Ok(await billingService.ToggleChargeItemActiveAsync(id, cancellationToken)); }
        catch (KeyNotFoundException) { return NotFound(); }
    }
}
