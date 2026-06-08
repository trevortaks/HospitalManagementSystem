using System.Net.Http.Headers;
using HospitalMS.Business.Models;
using HospitalMS.Web.Filters;
using Microsoft.AspNetCore.Mvc;

namespace HospitalMS.Web.Controllers;

[Route("reports")]
[RequireSession]
public sealed class ReportsController(IHttpClientFactory httpClientFactory) : Controller
{
    private const string TokenSessionKey = "jwt_token";

    [HttpGet("")]
    public async Task<IActionResult> Dashboard(CancellationToken ct)
    {
        var client = CreateAuthorizedClient();
        var summary = await client.GetFromJsonAsync<DashboardSummary>("/api/analytics/dashboard", ct);

        ViewData["Title"]      = "Dashboard";
        ViewData["ActivePage"] = "Reports";
        return View(summary);
    }

    [HttpGet("patients")]
    public async Task<IActionResult> Patients(CancellationToken ct)
    {
        var client = CreateAuthorizedClient();
        var report = await client.GetFromJsonAsync<PatientDemographicsReport>("/api/analytics/patients", ct);

        ViewData["Title"]      = "Patient Demographics";
        ViewData["ActivePage"] = "Reports";
        return View(report);
    }

    [HttpGet("revenue")]
    public async Task<IActionResult> Revenue(CancellationToken ct)
    {
        var client = CreateAuthorizedClient();
        var report = await client.GetFromJsonAsync<RevenueReport>("/api/analytics/revenue", ct);

        ViewData["Title"]      = "Revenue Report";
        ViewData["ActivePage"] = "Reports";
        return View(report);
    }

    [HttpGet("occupancy")]
    public async Task<IActionResult> Occupancy(CancellationToken ct)
    {
        var client = CreateAuthorizedClient();
        var report = await client.GetFromJsonAsync<BedOccupancyReport>("/api/analytics/bed-occupancy", ct);

        ViewData["Title"]      = "Bed Occupancy";
        ViewData["ActivePage"] = "Reports";
        return View(report);
    }

    [HttpGet("inventory")]
    public async Task<IActionResult> Inventory(CancellationToken ct)
    {
        var client = CreateAuthorizedClient();
        var report = await client.GetFromJsonAsync<InventoryStatusReport>("/api/analytics/inventory", ct);

        ViewData["Title"]      = "Inventory Status";
        ViewData["ActivePage"] = "Reports";
        return View(report);
    }

    private HttpClient CreateAuthorizedClient()
    {
        var client = httpClientFactory.CreateClient("HospitalAPI");
        var token = HttpContext.Session.GetString(TokenSessionKey);
        if (!string.IsNullOrEmpty(token))
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }
}
