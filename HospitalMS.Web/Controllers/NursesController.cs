using HospitalMS.Business.Models;
using HospitalMS.Common.Constants;
using HospitalMS.Web.Filters;
using HospitalMS.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace HospitalMS.Web.Controllers;

[Route("nurses")]
[RequireSession]
public sealed class NursesController(IHttpClientFactory f) : AppController(f)
{

    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var client   = Api();
        var allUsers = await client.GetFromJsonAsync<List<UserSummary>>("/api/users", ct) ?? [];
        var nurses   = allUsers.Where(u => u.Role == UserRoles.Nurse).ToList();

        ViewData["Title"]      = "Nurses";
        ViewData["ActivePage"] = "Nurses";
        return View(nurses);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Details(Guid id, CancellationToken ct)
    {
        var client = Api();

        var staffTask      = client.GetFromJsonAsync<UserSummary>($"/api/users/{id}", ct);
        var encountersTask = client.GetFromJsonAsync<IReadOnlyList<EncounterResponse>>($"/api/encounters?attendingDoctorId={id}", ct);
        var employeesTask  = client.GetFromJsonAsync<IReadOnlyList<EmployeeRecordResponse>>($"/api/hr/employees?userId={id}", ct);

        await Task.WhenAll(staffTask, encountersTask, employeesTask);

        var staff = await staffTask;
        if (staff is null) return NotFound();

        var employeeRecord = (await employeesTask ?? []).FirstOrDefault();

        IReadOnlyList<LeaveRequestResponse> leaveRequests = [];
        if (employeeRecord is not null)
            leaveRequests = await client.GetFromJsonAsync<IReadOnlyList<LeaveRequestResponse>>(
                $"/api/hr/leave-requests?employeeRecordId={employeeRecord.Id}", ct) ?? [];

        ViewData["ActivePage"] = "Nurses";
        return View(new StaffDetailViewModel
        {
            Staff          = staff,
            EmployeeRecord = employeeRecord,
            Appointments   = [],
            Encounters     = await encountersTask ?? [],
            LeaveRequests  = leaveRequests
        });
    }

    [HttpPost("create")]
    public async Task<IActionResult> Create([FromForm] RegisterRequest request, CancellationToken ct)
    {
        var client   = Api();
        var payload  = request with { Role = UserRoles.Nurse };
        var response = await client.PostAsJsonAsync("/api/users", payload, ct);

        if (!response.IsSuccessStatusCode)
            TempData["Error"] = "Failed to create nurse account. Username may already be taken.";
        else
            TempData["SuccessMessage"] = "Nurse account created successfully.";

        return RedirectToAction(nameof(Index));
    }

    [HttpGet("{id:guid}/edit")]
    public async Task<IActionResult> Edit(Guid id, CancellationToken ct)
    {
        var client = Api();
        var nurse  = await client.GetFromJsonAsync<UserSummary>($"/api/users/{id}", ct);
        if (nurse is null) return NotFound();

        ViewData["ActivePage"] = "Nurses";
        ViewData["NurseId"]    = id;
        ViewData["NurseName"]  = $"{nurse.FirstName} {nurse.LastName}".Trim().Length > 0
            ? $"{nurse.FirstName} {nurse.LastName}".Trim()
            : nurse.Username;

        return View(new UpdateUserProfileRequest(
            nurse.FirstName, nurse.LastName, nurse.PhoneNumber,
            nurse.AddressLine1, nurse.City, nurse.PostalCode, nurse.Country,
            nurse.Specialization, nurse.LicenseNumber, nurse.Bio));
    }

    [HttpPost("{id:guid}/edit")]
    public async Task<IActionResult> Edit(Guid id, [FromForm] UpdateUserProfileRequest request, CancellationToken ct)
    {
        var client   = Api();
        var response = await client.PutAsJsonAsync($"/api/users/{id}/profile", request, ct);

        if (!response.IsSuccessStatusCode)
            TempData["Error"] = "Failed to update nurse profile.";
        else
            TempData["SuccessMessage"] = "Nurse profile updated.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost("{id:guid}/toggle")]
    public async Task<IActionResult> Toggle(Guid id, CancellationToken ct)
    {
        var client = Api();
        await client.PostAsJsonAsync($"/api/users/{id}/toggle-active", new { }, ct);
        TempData["SuccessMessage"] = "Nurse status updated.";
        return RedirectToAction(nameof(Index));
    }

}
