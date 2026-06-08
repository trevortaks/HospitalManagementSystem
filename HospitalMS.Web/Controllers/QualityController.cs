using System.Net.Http.Headers;
using HospitalMS.Business.Models;
using HospitalMS.Web.Filters;
using Microsoft.AspNetCore.Mvc;

namespace HospitalMS.Web.Controllers;

[Route("quality")]
[RequireSession]
public sealed class QualityController(IHttpClientFactory httpClientFactory) : Controller
{
    private const string TokenSessionKey = "jwt_token";

    [HttpGet("")]
    public async Task<IActionResult> Incidents(
        [FromQuery] string? status, [FromQuery] string? severity, CancellationToken ct)
    {
        var client = CreateAuthorizedClient();
        var qs = new List<string>();
        if (!string.IsNullOrEmpty(status))   qs.Add($"status={Uri.EscapeDataString(status)}");
        if (!string.IsNullOrEmpty(severity)) qs.Add($"severity={Uri.EscapeDataString(severity)}");
        var url = "/api/quality/incidents" + (qs.Count > 0 ? "?" + string.Join("&", qs) : "");
        var incidents = await client.GetFromJsonAsync<List<QualityIncidentResponse>>(url, ct)
                        ?? [];

        ViewData["Title"]      = "Quality Incidents";
        ViewData["ActivePage"] = "Quality";
        ViewData["CurrentStatus"]   = status;
        ViewData["CurrentSeverity"] = severity;
        return View(incidents);
    }

    [HttpGet("feedback")]
    public async Task<IActionResult> Feedback(CancellationToken ct)
    {
        var client   = CreateAuthorizedClient();
        var feedback = await client.GetFromJsonAsync<List<PatientFeedbackResponse>>("/api/quality/feedback", ct)
                       ?? [];
        var summary  = await client.GetFromJsonAsync<QualitySummary>("/api/quality/summary", ct);

        ViewData["Title"]      = "Patient Feedback";
        ViewData["ActivePage"] = "Quality";
        ViewData["Summary"]    = summary;
        return View(feedback);
    }

    [HttpPost("incidents/create")]
    public async Task<IActionResult> CreateIncident([FromForm] CreateQualityIncidentRequest request, CancellationToken ct)
    {
        var client   = CreateAuthorizedClient();
        var response = await client.PostAsJsonAsync("/api/quality/incidents", request, ct);
        return RedirectToAction(nameof(Incidents));
    }

    [HttpPost("incidents/{id:guid}/assign")]
    public async Task<IActionResult> Assign(Guid id, [FromForm] AssignQualityIncidentRequest request, CancellationToken ct)
    {
        var client = CreateAuthorizedClient();
        await client.PostAsJsonAsync($"/api/quality/incidents/{id}/assign", request, ct);
        return RedirectToAction(nameof(Incidents));
    }

    [HttpPost("incidents/{id:guid}/resolve")]
    public async Task<IActionResult> Resolve(Guid id, [FromForm] ResolveQualityIncidentRequest request, CancellationToken ct)
    {
        var client = CreateAuthorizedClient();
        await client.PostAsJsonAsync($"/api/quality/incidents/{id}/resolve", request, ct);
        return RedirectToAction(nameof(Incidents));
    }

    [HttpPost("incidents/{id:guid}/close")]
    public async Task<IActionResult> Close(Guid id, CancellationToken ct)
    {
        var client = CreateAuthorizedClient();
        await client.PostAsJsonAsync($"/api/quality/incidents/{id}/close", new { }, ct);
        return RedirectToAction(nameof(Incidents));
    }

    [HttpPost("feedback/submit")]
    public async Task<IActionResult> SubmitFeedback([FromForm] CreatePatientFeedbackRequest request, CancellationToken ct)
    {
        var client = CreateAuthorizedClient();
        await client.PostAsJsonAsync("/api/quality/feedback", request, ct);
        return RedirectToAction(nameof(Feedback));
    }

    private HttpClient CreateAuthorizedClient()
    {
        var client = httpClientFactory.CreateClient("HospitalAPI");
        var token  = HttpContext.Session.GetString(TokenSessionKey);
        if (!string.IsNullOrEmpty(token))
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }
}
