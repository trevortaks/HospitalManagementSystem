using System.Net.Http.Headers;
using HospitalMS.Business.Models;
using HospitalMS.Web.Filters;
using Microsoft.AspNetCore.Mvc;

namespace HospitalMS.Web.Controllers;

[Route("hr")]
[RequireSession]
public sealed class HRController(IHttpClientFactory httpClientFactory) : Controller
{
    private const string TokenSessionKey = "jwt_token";

    [HttpGet("")]
    public async Task<IActionResult> Employees(
        [FromQuery] string? status, [FromQuery] Guid? departmentId, CancellationToken ct)
    {
        var client = CreateAuthorizedClient();
        var qs = new List<string>();
        if (!string.IsNullOrEmpty(status))   qs.Add($"status={Uri.EscapeDataString(status)}");
        if (departmentId.HasValue)           qs.Add($"departmentId={departmentId}");

        var url       = "/api/hr/employees" + (qs.Count > 0 ? "?" + string.Join("&", qs) : "");
        var employees = await client.GetFromJsonAsync<List<EmployeeRecordResponse>>(url, ct) ?? [];
        var depts     = await client.GetFromJsonAsync<List<DepartmentResponse>>("/api/hr/departments", ct) ?? [];

        ViewData["Title"]             = "Employees";
        ViewData["ActivePage"]        = "HR";
        ViewData["CurrentStatus"]     = status;
        ViewData["CurrentDepartment"] = departmentId;
        ViewData["Departments"]       = depts;
        return View(employees);
    }

    [HttpGet("departments")]
    public async Task<IActionResult> Departments(CancellationToken ct)
    {
        var client = CreateAuthorizedClient();
        var depts  = await client.GetFromJsonAsync<List<DepartmentResponse>>("/api/hr/departments", ct) ?? [];

        ViewData["Title"]      = "Departments";
        ViewData["ActivePage"] = "HR";
        return View(depts);
    }

    [HttpGet("leave")]
    public async Task<IActionResult> Leave([FromQuery] string? status, CancellationToken ct)
    {
        var client = CreateAuthorizedClient();
        var qs     = string.IsNullOrEmpty(status) ? "" : $"?status={Uri.EscapeDataString(status)}";
        var leaves = await client.GetFromJsonAsync<List<LeaveRequestResponse>>($"/api/hr/leave-requests{qs}", ct) ?? [];

        ViewData["Title"]         = "Leave Requests";
        ViewData["ActivePage"]    = "HR";
        ViewData["CurrentStatus"] = status;
        return View(leaves);
    }

    [HttpPost("departments/create")]
    public async Task<IActionResult> CreateDepartment([FromForm] CreateDepartmentRequest request, CancellationToken ct)
    {
        var client = CreateAuthorizedClient();
        await client.PostAsJsonAsync("/api/hr/departments", request, ct);
        return RedirectToAction(nameof(Departments));
    }

    [HttpPost("departments/{id:guid}/toggle")]
    public async Task<IActionResult> ToggleDepartment(Guid id, CancellationToken ct)
    {
        var client = CreateAuthorizedClient();
        await client.PostAsJsonAsync($"/api/hr/departments/{id}/toggle-active", new { }, ct);
        return RedirectToAction(nameof(Departments));
    }

    [HttpPost("employees/create")]
    public async Task<IActionResult> CreateEmployee([FromForm] CreateEmployeeRecordRequest request, CancellationToken ct)
    {
        var client = CreateAuthorizedClient();
        await client.PostAsJsonAsync("/api/hr/employees", request, ct);
        return RedirectToAction(nameof(Employees));
    }

    [HttpPost("employees/{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromForm] UpdateEmployeeStatusRequest request, CancellationToken ct)
    {
        var client = CreateAuthorizedClient();
        await client.PostAsJsonAsync($"/api/hr/employees/{id}/status", request, ct);
        return RedirectToAction(nameof(Employees));
    }

    [HttpPost("leave/create")]
    public async Task<IActionResult> CreateLeave([FromForm] CreateLeaveRequestRequest request, CancellationToken ct)
    {
        var client = CreateAuthorizedClient();
        await client.PostAsJsonAsync("/api/hr/leave-requests", request, ct);
        return RedirectToAction(nameof(Leave));
    }

    [HttpPost("leave/{id:guid}/review")]
    public async Task<IActionResult> ReviewLeave(Guid id, [FromForm] ReviewLeaveRequestRequest request, CancellationToken ct)
    {
        var client = CreateAuthorizedClient();
        await client.PostAsJsonAsync($"/api/hr/leave-requests/{id}/review", request, ct);
        return RedirectToAction(nameof(Leave));
    }

    [HttpPost("leave/{id:guid}/cancel")]
    public async Task<IActionResult> CancelLeave(Guid id, CancellationToken ct)
    {
        var client = CreateAuthorizedClient();
        await client.PostAsJsonAsync($"/api/hr/leave-requests/{id}/cancel", new { }, ct);
        return RedirectToAction(nameof(Leave));
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
