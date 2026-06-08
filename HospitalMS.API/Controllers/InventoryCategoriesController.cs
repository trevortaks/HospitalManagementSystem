using HospitalMS.Business.Models;
using HospitalMS.Business.Services;
using HospitalMS.Common.Auth;
using HospitalMS.Common.Constants;
using Microsoft.AspNetCore.Mvc;

namespace HospitalMS.API.Controllers;

[ApiController]
[Route("api/inventory/categories")]
[RoleBasedAuth(UserRoles.Admin, UserRoles.Pharmacist)]
public sealed class InventoryCategoriesController(IInventoryService inventoryService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool? activeOnly, CancellationToken cancellationToken)
    {
        var cats = await inventoryService.GetAllCategoriesAsync(activeOnly, cancellationToken);
        return Ok(cats);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var cat = await inventoryService.GetCategoryByIdAsync(id, cancellationToken);
        return cat is null ? NotFound() : Ok(cat);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateInventoryCategoryRequest request, CancellationToken cancellationToken)
    {
        var cat = await inventoryService.CreateCategoryAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = cat.Id }, cat);
    }

    [HttpPatch("{id:guid}/toggle-active")]
    public async Task<IActionResult> ToggleActive(Guid id, CancellationToken cancellationToken)
    {
        try { return Ok(await inventoryService.ToggleCategoryActiveAsync(id, cancellationToken)); }
        catch (KeyNotFoundException) { return NotFound(); }
    }
}
