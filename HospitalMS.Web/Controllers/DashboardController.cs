using System.Net.Http.Headers;
using HospitalMS.Business.Models;
using HospitalMS.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace HospitalMS.Web.Controllers;

[Route("dashboard")]
public sealed class DashboardController(IHttpClientFactory httpClientFactory) : Controller
{
    private const string TokenSessionKey = "jwt_token";

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var client = CreateAuthorizedClient();

        var patients = await client.GetFromJsonAsync<IReadOnlyList<PatientResponse>>(
            "/api/patients", cancellationToken) ?? [];

        IReadOnlyList<UserSummary> staff = [];
        var usersResponse = await client.GetAsync("/api/users", cancellationToken);
        if (usersResponse.IsSuccessStatusCode)
            staff = await usersResponse.Content.ReadFromJsonAsync<IReadOnlyList<UserSummary>>(cancellationToken) ?? [];

        var vm = new DashboardViewModel
        {
            TotalPatients = patients.Count,
            TotalDoctors = staff.Count(u => u.Role == "Doctor"),
            TotalNurses = staff.Count(u => u.Role == "Nurse"),
            TotalStaff = staff.Count,
            RecentPatients = [.. patients.OrderByDescending(p => p.CreatedAtUtc).Take(6)],
            RecentStaff = [.. staff.Take(6)]
        };

        return View(vm);
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
