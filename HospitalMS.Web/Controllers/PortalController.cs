using System.Net.Http.Headers;
using HospitalMS.Business.Models;
using Microsoft.AspNetCore.Mvc;

namespace HospitalMS.Web.Controllers;

[Route("portal")]
public sealed class PortalController(IHttpClientFactory httpClientFactory) : Controller
{
    private const string TokenSessionKey = "jwt_token";

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var client = CreateAuthorizedClient();
        var response = await client.GetAsync("/api/portal/dashboard", cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            ViewBag.NoLinkedPatient = true;
            return View(default(PortalDashboardResponse));
        }

        var dashboard = await response.Content.ReadFromJsonAsync<PortalDashboardResponse>(cancellationToken);

        // Log the portal access session
        await TryLogSessionAsync(client, dashboard?.Profile.PatientId, cancellationToken);

        return View(dashboard);
    }

    [HttpGet("appointments")]
    public async Task<IActionResult> Appointments(CancellationToken cancellationToken)
    {
        var client = CreateAuthorizedClient();
        var appointments = await client.GetFromJsonAsync<IReadOnlyList<PortalAppointmentResponse>>(
            "/api/portal/appointments", cancellationToken) ?? [];
        return View(appointments);
    }

    [HttpGet("prescriptions")]
    public async Task<IActionResult> Prescriptions(CancellationToken cancellationToken)
    {
        var client = CreateAuthorizedClient();
        var prescriptions = await client.GetFromJsonAsync<IReadOnlyList<PortalPrescriptionResponse>>(
            "/api/portal/prescriptions", cancellationToken) ?? [];
        return View(prescriptions);
    }

    [HttpGet("profile")]
    public async Task<IActionResult> Profile(CancellationToken cancellationToken)
    {
        var client = CreateAuthorizedClient();
        var response = await client.GetAsync("/api/portal/profile", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            ViewBag.NoLinkedPatient = true;
            return View(default(PortalProfileResponse));
        }

        var profile = await response.Content.ReadFromJsonAsync<PortalProfileResponse>(cancellationToken);
        return View(profile);
    }

    [HttpPost("profile")]
    public async Task<IActionResult> Profile(UpdatePortalProfileRequest request, CancellationToken cancellationToken)
    {
        var client = CreateAuthorizedClient();
        var response = await client.PatchAsJsonAsync("/api/portal/profile", request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError(string.Empty, "Failed to update profile.");
            var existing = await client.GetFromJsonAsync<PortalProfileResponse>("/api/portal/profile", cancellationToken);
            return View(existing);
        }

        TempData["SuccessMessage"] = "Profile updated successfully.";
        return RedirectToAction(nameof(Profile));
    }

    private async Task TryLogSessionAsync(HttpClient client, Guid? patientId, CancellationToken cancellationToken)
    {
        if (patientId is null) return;
        try
        {
            var request = new LogPortalSessionRequest(
                Guid.Empty, // userId resolved server-side from JWT
                patientId.Value,
                HttpContext.Connection.RemoteIpAddress?.ToString(),
                Request.Headers.UserAgent.ToString());
            await client.PostAsJsonAsync("/api/portal/session", request, cancellationToken);
        }
        catch
        {
            // Session logging is non-critical; never throw
        }
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
