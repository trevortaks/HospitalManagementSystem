using System.Net.Http.Headers;
using HospitalMS.Business.Models;
using Microsoft.AspNetCore.Mvc;

namespace HospitalMS.Web.Controllers;

[Route("encounters")]
public sealed class EncountersController(IHttpClientFactory httpClientFactory) : Controller
{
    private const string TokenSessionKey = "jwt_token";

    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] Guid? patientId, CancellationToken cancellationToken)
    {
        var client = CreateAuthorizedClient();
        var qs = patientId.HasValue ? $"?patientId={patientId}" : string.Empty;
        var encounters = await client.GetFromJsonAsync<IReadOnlyList<EncounterResponse>>(
            $"/api/encounters{qs}", cancellationToken) ?? [];

        if (patientId.HasValue)
        {
            var patient = await client.GetFromJsonAsync<PatientResponse>(
                $"/api/patients/{patientId}", cancellationToken);
            ViewBag.Patient = patient;
        }

        return View(encounters);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Detail(Guid id, CancellationToken cancellationToken)
    {
        var client = CreateAuthorizedClient();
        var encounter = await client.GetFromJsonAsync<EncounterResponse>(
            $"/api/encounters/{id}", cancellationToken);

        if (encounter is null) return NotFound();
        return View(encounter);
    }

    [HttpGet("create")]
    public async Task<IActionResult> Create([FromQuery] Guid? patientId, [FromQuery] Guid? appointmentId, CancellationToken cancellationToken)
    {
        var client = CreateAuthorizedClient();
        var patients = await client.GetFromJsonAsync<IReadOnlyList<PatientResponse>>("/api/patients", cancellationToken) ?? [];
        var users = await client.GetFromJsonAsync<IReadOnlyList<UserSummary>>("/api/users", cancellationToken) ?? [];

        ViewBag.Patients = patients;
        ViewBag.Doctors = users.Where(u => u.Role is "Doctor" or "Administrator").ToList();
        ViewBag.PreselectedPatientId = patientId;
        ViewBag.PreselectedAppointmentId = appointmentId;

        return View(new CreateEncounterRequest(
            patientId ?? Guid.Empty,
            Guid.Empty,
            "Outpatient",
            appointmentId));
    }

    [HttpPost("create")]
    public async Task<IActionResult> Create(CreateEncounterRequest request, CancellationToken cancellationToken)
    {
        var client = CreateAuthorizedClient();
        var response = await client.PostAsJsonAsync("/api/encounters", request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError(string.Empty, "Failed to create encounter.");
            var patients = await client.GetFromJsonAsync<IReadOnlyList<PatientResponse>>("/api/patients", cancellationToken) ?? [];
            var users = await client.GetFromJsonAsync<IReadOnlyList<UserSummary>>("/api/users", cancellationToken) ?? [];
            ViewBag.Patients = patients;
            ViewBag.Doctors = users.Where(u => u.Role is "Doctor" or "Administrator").ToList();
            return View(request);
        }

        var created = await response.Content.ReadFromJsonAsync<EncounterResponse>(cancellationToken);
        return RedirectToAction("Detail", new { id = created!.Id });
    }

    [HttpGet("{id:guid}/edit")]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var client = CreateAuthorizedClient();
        var encounter = await client.GetFromJsonAsync<EncounterResponse>(
            $"/api/encounters/{id}", cancellationToken);

        if (encounter is null) return NotFound();

        ViewBag.Encounter = encounter;
        return View(new UpdateEncounterRequest(
            encounter.EncounterType,
            encounter.ChiefComplaint,
            encounter.HistoryOfPresentIllness,
            encounter.Examination,
            encounter.Assessment,
            encounter.Plan,
            encounter.FollowUpNotes));
    }

    [HttpPost("{id:guid}/edit")]
    public async Task<IActionResult> Edit(Guid id, UpdateEncounterRequest request, CancellationToken cancellationToken)
    {
        var client = CreateAuthorizedClient();
        var response = await client.PutAsJsonAsync($"/api/encounters/{id}", request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError(string.Empty, "Failed to update encounter.");
            var encounter = await client.GetFromJsonAsync<EncounterResponse>($"/api/encounters/{id}", cancellationToken);
            ViewBag.Encounter = encounter;
            return View(request);
        }

        return RedirectToAction("Detail", new { id });
    }

    [HttpPost("{id:guid}/close")]
    public async Task<IActionResult> Close(Guid id, CancellationToken cancellationToken)
    {
        var client = CreateAuthorizedClient();
        await client.PostAsync($"/api/encounters/{id}/close", null, cancellationToken);
        return RedirectToAction("Detail", new { id });
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
