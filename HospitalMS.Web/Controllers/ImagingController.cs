using System.Net.Http.Headers;
using HospitalMS.Business.Models;
using Microsoft.AspNetCore.Mvc;

namespace HospitalMS.Web.Controllers;

[Route("imaging")]
public sealed class ImagingController(IHttpClientFactory httpClientFactory) : Controller
{
    private const string TokenSessionKey = "jwt_token";

    [HttpGet]
    public async Task<IActionResult> Index(
        [FromQuery] Guid? patientId,
        [FromQuery] Guid? encounterId,
        [FromQuery] string? status,
        CancellationToken cancellationToken)
    {
        var client = CreateAuthorizedClient();
        var qs = BuildQueryString(
            ("patientId", patientId?.ToString()),
            ("encounterId", encounterId?.ToString()),
            ("status", status));
        var requests = await client.GetFromJsonAsync<IReadOnlyList<ImagingRequestResponse>>(
            $"/api/imaging-requests{qs}", cancellationToken) ?? [];
        ViewBag.PatientId = patientId;
        ViewBag.StatusFilter = status;
        return View(requests);
    }

    [HttpGet("create")]
    public async Task<IActionResult> Create(
        [FromQuery] Guid? encounterId,
        [FromQuery] Guid? patientId,
        CancellationToken cancellationToken)
    {
        var client = CreateAuthorizedClient();
        var users = await client.GetFromJsonAsync<IReadOnlyList<UserSummary>>("/api/users", cancellationToken) ?? [];
        ViewBag.Doctors = users.Where(u => u.Role is "Doctor" or "Administrator").ToList();
        ViewBag.PreselectedEncounterId = encounterId;
        ViewBag.PreselectedPatientId = patientId;

        return View(new CreateImagingRequestRequest(
            encounterId ?? Guid.Empty,
            patientId ?? Guid.Empty,
            Guid.Empty,
            string.Empty));
    }

    [HttpPost("create")]
    public async Task<IActionResult> Create(CreateImagingRequestRequest request, CancellationToken cancellationToken)
    {
        var client = CreateAuthorizedClient();
        var response = await client.PostAsJsonAsync("/api/imaging-requests", request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError(string.Empty, "Failed to create imaging request.");
            var users = await client.GetFromJsonAsync<IReadOnlyList<UserSummary>>("/api/users", cancellationToken) ?? [];
            ViewBag.Doctors = users.Where(u => u.Role is "Doctor" or "Administrator").ToList();
            return View(request);
        }
        return RedirectToAction(nameof(Index), new { patientId = request.PatientId });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> RequestDetail(Guid id, CancellationToken cancellationToken)
    {
        var client = CreateAuthorizedClient();
        var request = await client.GetFromJsonAsync<ImagingRequestResponse>($"/api/imaging-requests/{id}", cancellationToken);
        if (request is null) return NotFound();

        var users = await client.GetFromJsonAsync<IReadOnlyList<UserSummary>>("/api/users", cancellationToken) ?? [];
        ViewBag.Radiologists = users.Where(u => u.Role is "Radiologist" or "Administrator").ToList();
        return View(request);
    }

    [HttpPost("{id:guid}/report")]
    public async Task<IActionResult> CreateReport(Guid id, CreateImagingReportRequest request, CancellationToken cancellationToken)
    {
        var client = CreateAuthorizedClient();
        await client.PostAsJsonAsync($"/api/imaging-requests/{id}/report", request, cancellationToken);
        return RedirectToAction(nameof(RequestDetail), new { id });
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, Guid? patientId, CancellationToken cancellationToken)
    {
        var client = CreateAuthorizedClient();
        await client.PatchAsync($"/api/imaging-requests/{id}/cancel", null, cancellationToken);
        return RedirectToAction(nameof(Index), new { patientId });
    }

    [HttpPost("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromForm] string status, CancellationToken cancellationToken)
    {
        var client = CreateAuthorizedClient();
        await client.PatchAsJsonAsync($"/api/imaging-requests/{id}/status", status, cancellationToken);
        return RedirectToAction(nameof(RequestDetail), new { id });
    }

    private HttpClient CreateAuthorizedClient()
    {
        var client = httpClientFactory.CreateClient("HospitalAPI");
        var token = HttpContext.Session.GetString(TokenSessionKey);
        if (!string.IsNullOrEmpty(token))
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private static string BuildQueryString(params (string key, string? value)[] pairs)
    {
        var parts = pairs
            .Where(p => !string.IsNullOrEmpty(p.value))
            .Select(p => $"{p.key}={Uri.EscapeDataString(p.value!)}");
        var qs = string.Join("&", parts);
        return string.IsNullOrEmpty(qs) ? string.Empty : $"?{qs}";
    }
}
