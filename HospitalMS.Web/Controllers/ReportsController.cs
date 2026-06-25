using HospitalMS.Business.Models;
using HospitalMS.Web.Filters;
using Microsoft.AspNetCore.Mvc;

namespace HospitalMS.Web.Controllers;

[Route("reports")]
[RequireSession]
public sealed class ReportsController(IHttpClientFactory f) : AppController(f)
{

    [HttpGet("")]
    public async Task<IActionResult> Dashboard(CancellationToken ct)
    {
        var client = Api();
        var summary = await client.GetFromJsonAsync<DashboardSummary>("/api/analytics/dashboard", ct);

        ViewData["Title"]      = "Dashboard";
        ViewData["ActivePage"] = "Reports";
        return View(summary);
    }

    [HttpGet("patients")]
    public async Task<IActionResult> Patients(CancellationToken ct)
    {
        var client = Api();
        var report = await client.GetFromJsonAsync<PatientDemographicsReport>("/api/analytics/patients", ct);

        ViewData["Title"]      = "Patient Demographics";
        ViewData["ActivePage"] = "Reports";
        return View(report);
    }

    [HttpGet("revenue")]
    public async Task<IActionResult> Revenue(CancellationToken ct)
    {
        var client = Api();
        var report = await client.GetFromJsonAsync<RevenueReport>("/api/analytics/revenue", ct);

        ViewData["Title"]      = "Revenue Report";
        ViewData["ActivePage"] = "Reports";
        return View(report);
    }

    [HttpGet("occupancy")]
    public async Task<IActionResult> Occupancy(CancellationToken ct)
    {
        var client = Api();
        var report = await client.GetFromJsonAsync<BedOccupancyReport>("/api/analytics/bed-occupancy", ct);

        ViewData["Title"]      = "Bed Occupancy";
        ViewData["ActivePage"] = "Reports";
        return View(report);
    }

    [HttpGet("inventory")]
    public async Task<IActionResult> Inventory(CancellationToken ct)
    {
        var client = Api();
        var report = await client.GetFromJsonAsync<InventoryStatusReport>("/api/analytics/inventory", ct);

        ViewData["Title"]      = "Inventory Status";
        ViewData["ActivePage"] = "Reports";
        return View(report);
    }

}
