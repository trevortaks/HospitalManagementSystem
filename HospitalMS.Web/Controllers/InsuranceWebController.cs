using System.Net.Http.Headers;
using HospitalMS.Business.Models;
using HospitalMS.Web.Filters;
using Microsoft.AspNetCore.Mvc;

namespace HospitalMS.Web.Controllers;

[Route("insurance")]
[RequireSession]
public sealed class InsuranceWebController(IHttpClientFactory httpClientFactory) : Controller
{
    private const string TokenSessionKey = "jwt_token";

    [HttpGet("providers")]
    public async Task<IActionResult> Providers(CancellationToken cancellationToken)
    {
        var client = CreateAuthorizedClient();
        var providers = await client.GetFromJsonAsync<IReadOnlyList<InsuranceProviderResponse>>("/api/insurance/providers", cancellationToken) ?? [];
        ViewData["Title"] = "Insurance Providers";
        ViewData["ActivePage"] = "Insurance";
        return View(providers);
    }

    [HttpGet("providers/create")]
    public IActionResult CreateProvider()
    {
        ViewData["Title"] = "New Insurance Provider";
        ViewData["ActivePage"] = "Insurance";
        return View();
    }

    [HttpPost("providers/create")]
    public async Task<IActionResult> CreateProvider(CreateInsuranceProviderRequest request, CancellationToken cancellationToken)
    {
        var client = CreateAuthorizedClient();
        await client.PostAsJsonAsync("/api/insurance/providers", request, cancellationToken);
        return RedirectToAction(nameof(Providers));
    }

    [HttpPost("providers/{id:guid}/toggle")]
    public async Task<IActionResult> ToggleProvider(Guid id, CancellationToken cancellationToken)
    {
        var client = CreateAuthorizedClient();
        await client.PatchAsync($"/api/insurance/providers/{id}/toggle-active", null, cancellationToken);
        return RedirectToAction(nameof(Providers));
    }

    [HttpGet("patient/{patientId:guid}")]
    public async Task<IActionResult> PatientCoverage(Guid patientId, CancellationToken cancellationToken)
    {
        var client = CreateAuthorizedClient();
        var coverage = await client.GetFromJsonAsync<IReadOnlyList<PatientInsuranceResponse>>(
            $"/api/insurance/patient/{patientId}", cancellationToken) ?? [];
        var providers = await client.GetFromJsonAsync<IReadOnlyList<InsuranceProviderResponse>>(
            "/api/insurance/providers?activeOnly=true", cancellationToken) ?? [];

        ViewBag.PatientId = patientId;
        ViewBag.Providers = providers;
        ViewData["Title"] = "Patient Coverage";
        ViewData["ActivePage"] = "Insurance";
        return View(coverage);
    }

    [HttpPost("patient/add")]
    public async Task<IActionResult> AddPatientInsurance(AddPatientInsuranceRequest request, CancellationToken cancellationToken)
    {
        var client = CreateAuthorizedClient();
        await client.PostAsJsonAsync("/api/insurance/patient", request, cancellationToken);
        return RedirectToAction(nameof(PatientCoverage), new { patientId = request.PatientId });
    }

    [HttpPost("patient/{id:guid}/set-primary")]
    public async Task<IActionResult> SetPrimary(Guid id, [FromForm] Guid patientId, CancellationToken cancellationToken)
    {
        var client = CreateAuthorizedClient();
        await client.PatchAsync($"/api/insurance/patient/{id}/set-primary", null, cancellationToken);
        return RedirectToAction(nameof(PatientCoverage), new { patientId });
    }

    [HttpPost("patient/{id:guid}/delete")]
    public async Task<IActionResult> DeletePatientInsurance(Guid id, [FromForm] Guid patientId, CancellationToken cancellationToken)
    {
        var client = CreateAuthorizedClient();
        await client.DeleteAsync($"/api/insurance/patient/{id}", cancellationToken);
        return RedirectToAction(nameof(PatientCoverage), new { patientId });
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
