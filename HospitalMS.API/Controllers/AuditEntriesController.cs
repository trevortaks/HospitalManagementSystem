using HospitalMS.Business.Models;
using HospitalMS.Business.Services;
using HospitalMS.Common.Auth;
using HospitalMS.Common.Constants;
using Microsoft.AspNetCore.Mvc;

namespace HospitalMS.API.Controllers;

[ApiController]
[Route("api/audit-entries")]
[RoleBasedAuth(UserRoles.Admin)]
public sealed class AuditEntriesController(IAuditService auditService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetEntries(
        [FromQuery] string? entityType,
        [FromQuery] Guid? entityId,
        [FromQuery] string? action,
        [FromQuery] Guid? userId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int limit = 200,
        CancellationToken cancellationToken = default)
    {
        var filter = new AuditEntryFilter(entityType, entityId, action, userId, from, to);
        var entries = await auditService.GetEntriesAsync(filter, Math.Clamp(limit, 1, 500), cancellationToken);
        return Ok(entries);
    }

    [HttpGet("entity/{entityType}/{entityId:guid}")]
    public async Task<IActionResult> GetByEntity(string entityType, Guid entityId, CancellationToken cancellationToken)
    {
        var entries = await auditService.GetEntriesByEntityAsync(entityType, entityId, cancellationToken);
        return Ok(entries);
    }

    [HttpPost]
    public async Task<IActionResult> CreateEntry([FromBody] CreateAuditEntryRequest request, CancellationToken cancellationToken)
    {
        var result = await auditService.LogAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetEntries), new { }, result);
    }
}
