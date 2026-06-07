using System.Net.Http.Headers;
using HospitalMS.Business.Models;
using Microsoft.AspNetCore.Mvc;

namespace HospitalMS.Web.Controllers;

[Route("medications")]
public sealed class MedicationsController(IHttpClientFactory httpClientFactory) : Controller
{
    private const string TokenSessionKey = "jwt_token";

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var client = CreateAuthorizedClient();
        var meds = await client.GetFromJsonAsync<IReadOnlyList<MedicationResponse>>(
            "/api/medications", cancellationToken) ?? [];
        return View(meds);
    }

    [HttpGet("create")]
    public IActionResult Create() =>
        View(new CreateMedicationRequest(string.Empty, string.Empty));

    [HttpPost("create")]
    public async Task<IActionResult> Create(CreateMedicationRequest request, CancellationToken cancellationToken)
    {
        var client = CreateAuthorizedClient();
        var response = await client.PostAsJsonAsync("/api/medications", request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError(string.Empty, "Failed to create medication.");
            return View(request);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet("{id:guid}/edit")]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var client = CreateAuthorizedClient();
        var med = await client.GetFromJsonAsync<MedicationResponse>($"/api/medications/{id}", cancellationToken);
        if (med is null) return NotFound();

        ViewBag.MedicationId = id;
        return View(new UpdateMedicationRequest(
            med.GenericName, med.Form, med.BrandName, med.Strength,
            med.RouteOfAdministration, med.IsControlled, med.IsActive));
    }

    [HttpPost("{id:guid}/edit")]
    public async Task<IActionResult> Edit(Guid id, UpdateMedicationRequest request, CancellationToken cancellationToken)
    {
        var client = CreateAuthorizedClient();
        var response = await client.PutAsJsonAsync($"/api/medications/{id}", request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError(string.Empty, "Failed to update medication.");
            ViewBag.MedicationId = id;
            return View(request);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost("{id:guid}/toggle-active")]
    public async Task<IActionResult> ToggleActive(Guid id, CancellationToken cancellationToken)
    {
        var client = CreateAuthorizedClient();
        await client.PatchAsync($"/api/medications/{id}/toggle-active", null, cancellationToken);
        return RedirectToAction(nameof(Index));
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
