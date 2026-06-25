using HospitalMS.Business.Models;
using HospitalMS.Common.Constants;
using HospitalMS.Web.Filters;
using HospitalMS.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace HospitalMS.Web.Controllers;

[Route("doctors")]
[RequireSession]
public sealed class DoctorsController(IHttpClientFactory f) : AppController(f)
{

    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var client  = Api();
        var allUsers = await client.GetFromJsonAsync<List<UserSummary>>("/api/users", ct) ?? [];
        var doctors  = allUsers.Where(u => u.Role == UserRoles.Doctor).ToList();

        ViewData["Title"]      = "Doctors";
        ViewData["ActivePage"] = "Doctors";
        return View(doctors);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Details(Guid id, CancellationToken ct)
    {
        var client = Api();

        var staffTask       = client.GetFromJsonAsync<UserSummary>($"/api/users/{id}", ct);
        var appointmentsTask= client.GetFromJsonAsync<IReadOnlyList<AppointmentResponse>>($"/api/appointments?doctorUserId={id}", ct);
        var encountersTask  = client.GetFromJsonAsync<IReadOnlyList<EncounterResponse>>($"/api/encounters?attendingDoctorId={id}", ct);
        var employeesTask   = client.GetFromJsonAsync<IReadOnlyList<EmployeeRecordResponse>>($"/api/hr/employees?userId={id}", ct);

        await Task.WhenAll(staffTask, appointmentsTask, encountersTask, employeesTask);

        var staff = await staffTask;
        if (staff is null) return NotFound();

        var employeeRecord = (await employeesTask ?? []).FirstOrDefault();

        IReadOnlyList<LeaveRequestResponse> leaveRequests = [];
        if (employeeRecord is not null)
            leaveRequests = await client.GetFromJsonAsync<IReadOnlyList<LeaveRequestResponse>>(
                $"/api/hr/leave-requests?employeeRecordId={employeeRecord.Id}", ct) ?? [];

        ViewData["ActivePage"] = "Doctors";
        return View(new StaffDetailViewModel
        {
            Staff          = staff,
            EmployeeRecord = employeeRecord,
            Appointments   = await appointmentsTask ?? [],
            Encounters     = await encountersTask ?? [],
            LeaveRequests  = leaveRequests
        });
    }

    [HttpPost("create")]
    public async Task<IActionResult> Create([FromForm] RegisterRequest request, CancellationToken ct)
    {
        var client   = Api();
        var payload  = request with { Role = UserRoles.Doctor };
        var response = await client.PostAsJsonAsync("/api/users", payload, ct);

        if (!response.IsSuccessStatusCode)
            TempData["Error"] = "Failed to create doctor account. Username may already be taken.";
        else
            TempData["SuccessMessage"] = "Doctor account created successfully.";

        return RedirectToAction(nameof(Index));
    }

    [HttpGet("{id:guid}/edit")]
    public async Task<IActionResult> Edit(Guid id, CancellationToken ct)
    {
        var client = Api();
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
        var client = Api();
        var response = await client.PutAsJsonAsync($"/api/users/{id}/profile", request, ct);

        if (!response.IsSuccessStatusCode)
            TempData["Error"] = "Failed to update doctor profile.";
        else
            TempData["SuccessMessage"] = "Doctor profile updated.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost("{id:guid}/toggle")]
    public async Task<IActionResult> Toggle(Guid id, CancellationToken ct)
    {
        var client = Api();
        await client.PostAsJsonAsync($"/api/users/{id}/toggle-active", new { }, ct);
        TempData["SuccessMessage"] = "Doctor status updated.";
        return RedirectToAction(nameof(Index));
    }

}
