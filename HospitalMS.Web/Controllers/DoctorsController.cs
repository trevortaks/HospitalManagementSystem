using System.Net.Http.Headers;
using HospitalMS.Business.Models;
using HospitalMS.Common.Constants;
using HospitalMS.Web.Filters;
using Microsoft.AspNetCore.Mvc;

namespace HospitalMS.Web.Controllers;

[Route("doctors")]
[RequireSession]
public sealed class DoctorsController(IHttpClientFactory httpClientFactory) : Controller
{
    private const string TokenSessionKey = "jwt_token";

    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var client  = CreateAuthorizedClient();
        var allUsers = await client.GetFromJsonAsync<List<UserSummary>>("/api/users", ct) ?? [];
        var doctors  = allUsers.Where(u => u.Role == UserRoles.Doctor).ToList();

        ViewData["Title"]      = "Doctors";
        ViewData["ActivePage"] = "Doctors";
        return View(doctors);
    }

    [HttpPost("create")]
    public async Task<IActionResult> Create([FromForm] RegisterRequest request, CancellationToken ct)
    {
        var client   = CreateAuthorizedClient();
        var payload  = request with { Role = UserRoles.Doctor };
        var response = await client.PostAsJsonAsync("/api/auth/register", payload, ct);

        if (!response.IsSuccessStatusCode)
            TempData["Error"] = "Failed to create doctor account. Username may already be taken.";

        return RedirectToAction(nameof(Index));
    }

    [HttpGet("{id:guid}/edit")]
    public async Task<IActionResult> Edit(Guid id, CancellationToken ct)
    {
        var client = CreateAuthorizedClient();
        var doctor = await client.GetFromJsonAsync<UserSummary>($"/api/users/{id}", ct);
        if (doctor is null) return NotFound();

        ViewData["ActivePage"] = "Doctors";
        ViewData["DoctorId"] = id;
        ViewData["DoctorName"] = $"{doctor.FirstName} {doctor.LastName}".Trim().Length > 0
            ? $"{doctor.FirstName} {doctor.LastName}".Trim()
            : doctor.Username;

        return View(new UpdateUserProfileRequest(
            doctor.FirstName, doctor.LastName, doctor.PhoneNumber,
            doctor.AddressLine1, doctor.City, doctor.PostalCode, doctor.Country,
            doctor.Specialization, doctor.LicenseNumber, doctor.Bio));
    }

    [HttpPost("{id:guid}/edit")]
    public async Task<IActionResult> Edit(Guid id, [FromForm] UpdateUserProfileRequest request, CancellationToken ct)
    {
        var client = CreateAuthorizedClient();
        var response = await client.PutAsJsonAsync($"/api/users/{id}/profile", request, ct);

        if (!response.IsSuccessStatusCode)
            TempData["Error"] = "Failed to update doctor profile.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost("{id:guid}/toggle")]
    public async Task<IActionResult> Toggle(Guid id, CancellationToken ct)
    {
        var client = CreateAuthorizedClient();
        await client.PostAsJsonAsync($"/api/users/{id}/toggle-active", new { }, ct);
        return RedirectToAction(nameof(Index));
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
