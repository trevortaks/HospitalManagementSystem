using HospitalMS.Business.Models;
using HospitalMS.Business.Services;
using HospitalMS.Common.Auth;
using HospitalMS.Common.Constants;
using Microsoft.AspNetCore.Mvc;

namespace HospitalMS.API.Controllers;

[ApiController]
[Route("api/hr")]
[RoleBasedAuth(UserRoles.Admin, UserRoles.HRManager)]
public sealed class HRController(IHRService hrService) : ControllerBase
{
    [HttpGet("summary")]
    public async Task<IActionResult> Summary(CancellationToken ct)
        => Ok(await hrService.GetSummaryAsync(ct));

    // ── Departments ────────────────────────────────────────────────────────

    [HttpGet("departments")]
    public async Task<IActionResult> GetDepartments(CancellationToken ct)
        => Ok(await hrService.GetDepartmentsAsync(ct));

    [HttpPost("departments")]
    public async Task<IActionResult> CreateDepartment(
        [FromBody] CreateDepartmentRequest request, CancellationToken ct)
    {
        try { return Ok(await hrService.CreateDepartmentAsync(request, ct)); }
        catch (InvalidOperationException ex) { return Conflict(new { error = ex.Message }); }
    }

    [HttpPost("departments/{id:guid}/toggle-active")]
    public async Task<IActionResult> ToggleDepartmentActive(Guid id, CancellationToken ct)
    {
        try { return Ok(await hrService.ToggleDepartmentActiveAsync(id, ct)); }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    // ── Employees ──────────────────────────────────────────────────────────

    [HttpGet("employees")]
    public async Task<IActionResult> GetEmployees(
        [FromQuery] string? status, [FromQuery] Guid? departmentId, CancellationToken ct)
        => Ok(await hrService.GetEmployeesAsync(status, departmentId, ct));

    [HttpGet("employees/{id:guid}")]
    public async Task<IActionResult> GetEmployee(Guid id, CancellationToken ct)
    {
        try { return Ok(await hrService.GetEmployeeByIdAsync(id, ct)); }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    [HttpPost("employees")]
    public async Task<IActionResult> CreateEmployee(
        [FromBody] CreateEmployeeRecordRequest request, CancellationToken ct)
    {
        try
        {
            var result = await hrService.CreateEmployeeRecordAsync(request, ct);
            return CreatedAtAction(nameof(GetEmployee), new { id = result.Id }, result);
        }
        catch (InvalidOperationException ex) { return Conflict(new { error = ex.Message }); }
    }

    [HttpPost("employees/{id:guid}/status")]
    public async Task<IActionResult> UpdateEmployeeStatus(
        Guid id, [FromBody] UpdateEmployeeStatusRequest request, CancellationToken ct)
    {
        try { return Ok(await hrService.UpdateEmployeeStatusAsync(id, request, ct)); }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    // ── Leave Requests ─────────────────────────────────────────────────────

    [HttpGet("leave-requests")]
    public async Task<IActionResult> GetLeaveRequests(
        [FromQuery] string? status, [FromQuery] Guid? employeeRecordId, CancellationToken ct)
        => Ok(await hrService.GetLeaveRequestsAsync(status, employeeRecordId, ct));

    [HttpPost("leave-requests")]
    public async Task<IActionResult> CreateLeaveRequest(
        [FromBody] CreateLeaveRequestRequest request, CancellationToken ct)
    {
        try { return Ok(await hrService.CreateLeaveRequestAsync(request, ct)); }
        catch (ArgumentException ex) { return BadRequest(new { error = ex.Message }); }
    }

    [HttpPost("leave-requests/{id:guid}/review")]
    public async Task<IActionResult> ReviewLeaveRequest(
        Guid id, [FromBody] ReviewLeaveRequestRequest request, CancellationToken ct)
    {
        var userId = User.FindFirst("sub")?.Value;
        if (!Guid.TryParse(userId, out var reviewerId))
            return Unauthorized();

        try { return Ok(await hrService.ReviewLeaveRequestAsync(id, reviewerId, request, ct)); }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
    }

    [HttpPost("leave-requests/{id:guid}/cancel")]
    public async Task<IActionResult> CancelLeaveRequest(Guid id, CancellationToken ct)
    {
        try { return Ok(await hrService.CancelLeaveRequestAsync(id, ct)); }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
    }
}
