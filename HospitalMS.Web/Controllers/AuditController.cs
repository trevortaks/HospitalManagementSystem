using HospitalMS.Business.Models;
using HospitalMS.Web.Filters;
using Microsoft.AspNetCore.Mvc;

namespace HospitalMS.Web.Controllers;

[Route("audit")]
[RequireSession]
public sealed class AuditController(IHttpClientFactory f) : AppController(f)
{

    [HttpGet("")]
    public async Task<IActionResult> Index(
        [FromQuery] string? entityType,
        [FromQuery] string? action,
        [FromQuery] Guid? userId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken cancellationToken)
    {
        var client = Api();

        var qs = new List<string>();
        if (!string.IsNullOrWhiteSpace(entityType)) qs.Add($"entityType={Uri.EscapeDataString(entityType)}");
        if (!string.IsNullOrWhiteSpace(action))     qs.Add($"action={Uri.EscapeDataString(action)}");
        if (userId.HasValue)                         qs.Add($"userId={userId}");
        if (from.HasValue)                           qs.Add($"from={from.Value:o}");
        if (to.HasValue)                             qs.Add($"to={to.Value:o}");
        qs.Add("limit=200");

        var url = "/api/audit-entries" + (qs.Any() ? "?" + string.Join("&", qs) : "");
        var entries = await client.GetFromJsonAsync<IReadOnlyList<AuditEntryResponse>>(url, cancellationToken) ?? [];

        ViewBag.EntityType = entityType;
        ViewBag.Action     = action;
        ViewBag.UserId     = userId;
        ViewBag.From       = from;
        ViewBag.To         = to;
        ViewData["Title"]      = "Audit Log";
        ViewData["ActivePage"] = "Audit";
        return View(entries);
    }

}
