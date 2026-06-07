using System.Net.Http.Headers;
using HospitalMS.Business.Models;
using Microsoft.AspNetCore.Mvc;

namespace HospitalMS.Web.Controllers;

[Route("patients")]
public sealed class PatientsController(IHttpClientFactory httpClientFactory) : Controller
{
    private const string TokenSessionKey = "jwt_token";

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var client = CreateAuthorizedClient();
        var patients = await client.GetFromJsonAsync<IReadOnlyList<PatientResponse>>(
            "/api/patients", cancellationToken) ?? [];
        return View(patients);
    }

    [HttpGet("create")]
    public IActionResult Create() => View(new CreatePatientRequest(string.Empty, string.Empty, string.Empty, DateTime.Today, string.Empty));

    [HttpPost("create")]
    public async Task<IActionResult> Create(CreatePatientRequest request, CancellationToken cancellationToken)
    {
        var client = CreateAuthorizedClient();
        var response = await client.PostAsJsonAsync("/api/patients", request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError(string.Empty, "Failed to create patient.");
            return View(request);
        }

        return RedirectToAction("Index");
    }

    [HttpGet("{id:guid}/edit")]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var client = CreateAuthorizedClient();
        var patient = await client.GetFromJsonAsync<PatientResponse>(
            $"/api/patients/{id}", cancellationToken);

        if (patient is null)
            return NotFound();

        return View(new UpdatePatientRequest(
            patient.FirstName, patient.LastName, patient.DateOfBirth, patient.Email,
            patient.PhoneNumber, patient.Gender, patient.BloodGroup,
            patient.AddressLine1, patient.City, patient.PostalCode, patient.Country,
            patient.EmergencyContactName, patient.EmergencyContactPhone));
    }

    [HttpPost("{id:guid}/edit")]
    public async Task<IActionResult> Edit(Guid id, UpdatePatientRequest request, CancellationToken cancellationToken)
    {
        var client = CreateAuthorizedClient();
        var response = await client.PutAsJsonAsync($"/api/patients/{id}", request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError(string.Empty, "Failed to update patient.");
            return View(request);
        }

        return RedirectToAction("Index");
    }

    [HttpPost("{id:guid}/delete")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var client = CreateAuthorizedClient();
        await client.DeleteAsync($"/api/patients/{id}", cancellationToken);
        return RedirectToAction("Index");
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
