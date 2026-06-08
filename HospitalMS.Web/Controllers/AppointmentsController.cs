using System.Net.Http.Headers;
using HospitalMS.Business.Models;
using HospitalMS.Web.Filters;
using Microsoft.AspNetCore.Mvc;

namespace HospitalMS.Web.Controllers;

[Route("appointments")]
[RequireSession]
public sealed class AppointmentsController(IHttpClientFactory httpClientFactory) : Controller
{
    private const string TokenSessionKey = "jwt_token";

    [HttpGet]
    public async Task<IActionResult> Index(
        [FromQuery] Guid? patientId,
        [FromQuery] string? status,
        CancellationToken cancellationToken)
    {
        var client = CreateAuthorizedClient();
        var qs = BuildQueryString(("patientId", patientId?.ToString()), ("status", status));
        var appointments = await client.GetFromJsonAsync<IReadOnlyList<AppointmentResponse>>(
            $"/api/appointments{qs}", cancellationToken) ?? [];

        ViewBag.PatientId = patientId;
        ViewBag.StatusFilter = status;
        return View(appointments);
    }

    [HttpGet("create")]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var client = CreateAuthorizedClient();
        var patients = await client.GetFromJsonAsync<IReadOnlyList<PatientResponse>>(
            "/api/patients", cancellationToken) ?? [];
        var users = await client.GetFromJsonAsync<IReadOnlyList<UserSummary>>(
            "/api/users", cancellationToken) ?? [];

        ViewBag.Patients = patients;
        ViewBag.Doctors = users.Where(u => u.Role is "Doctor" or "Administrator").ToList();
        return View(new CreateAppointmentRequest(Guid.Empty, Guid.Empty, DateTime.Today.AddHours(9)));
    }

    [HttpPost("create")]
    public async Task<IActionResult> Create(CreateAppointmentRequest request, CancellationToken cancellationToken)
    {
        var client = CreateAuthorizedClient();
        var response = await client.PostAsJsonAsync("/api/appointments", request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError(string.Empty, "Failed to create appointment.");
            var patients = await client.GetFromJsonAsync<IReadOnlyList<PatientResponse>>("/api/patients", cancellationToken) ?? [];
            var users = await client.GetFromJsonAsync<IReadOnlyList<UserSummary>>("/api/users", cancellationToken) ?? [];
            ViewBag.Patients = patients;
            ViewBag.Doctors = users.Where(u => u.Role is "Doctor" or "Administrator").ToList();
            return View(request);
        }

        return RedirectToAction("Index");
    }

    [HttpGet("{id:guid}/edit")]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var client = CreateAuthorizedClient();
        var appointment = await client.GetFromJsonAsync<AppointmentResponse>(
            $"/api/appointments/{id}", cancellationToken);

        if (appointment is null) return NotFound();

        var patients = await client.GetFromJsonAsync<IReadOnlyList<PatientResponse>>("/api/patients", cancellationToken) ?? [];
        var users = await client.GetFromJsonAsync<IReadOnlyList<UserSummary>>("/api/users", cancellationToken) ?? [];

        ViewBag.Patients = patients;
        ViewBag.Doctors = users.Where(u => u.Role is "Doctor" or "Administrator").ToList();
        ViewBag.Appointment = appointment;
        ViewBag.AppointmentId = id;

        return View(new UpdateAppointmentRequest(
            appointment.ScheduledAtUtc, appointment.DurationMinutes,
            appointment.Type, appointment.Reason, appointment.Notes));
    }

    [HttpPost("{id:guid}/edit")]
    public async Task<IActionResult> Edit(Guid id, UpdateAppointmentRequest request, CancellationToken cancellationToken)
    {
        var client = CreateAuthorizedClient();
        var response = await client.PutAsJsonAsync($"/api/appointments/{id}", request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError(string.Empty, "Failed to update appointment.");
            var patients = await client.GetFromJsonAsync<IReadOnlyList<PatientResponse>>("/api/patients", cancellationToken) ?? [];
            var users = await client.GetFromJsonAsync<IReadOnlyList<UserSummary>>("/api/users", cancellationToken) ?? [];
            ViewBag.Patients = patients;
            ViewBag.Doctors = users.Where(u => u.Role is "Doctor" or "Administrator").ToList();
            ViewBag.AppointmentId = id;
            return View(request);
        }

        return RedirectToAction("Index");
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, string? reason, CancellationToken cancellationToken)
    {
        var client = CreateAuthorizedClient();
        await client.PatchAsJsonAsync($"/api/appointments/{id}/cancel",
            new CancelAppointmentRequest(reason), cancellationToken);
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

    private static string BuildQueryString(params (string key, string? value)[] pairs)
    {
        var parts = pairs
            .Where(p => !string.IsNullOrEmpty(p.value))
            .Select(p => $"{p.key}={Uri.EscapeDataString(p.value!)}");
        var qs = string.Join("&", parts);
        return string.IsNullOrEmpty(qs) ? string.Empty : $"?{qs}";
    }
}
