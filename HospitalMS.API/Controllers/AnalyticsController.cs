using HospitalMS.Business.Services;
using HospitalMS.Common.Auth;
using HospitalMS.Common.Constants;
using Microsoft.AspNetCore.Mvc;

namespace HospitalMS.API.Controllers;

[ApiController]
[Route("api/analytics")]
[RoleBasedAuth(UserRoles.Admin, UserRoles.Doctor, UserRoles.AccountsManager)]
public sealed class AnalyticsController(IAnalyticsService analyticsService) : ControllerBase
{
    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard(CancellationToken ct)
        => Ok(await analyticsService.GetDashboardSummaryAsync(ct));

    [HttpGet("patients")]
    public async Task<IActionResult> Patients(CancellationToken ct)
        => Ok(await analyticsService.GetPatientDemographicsAsync(ct));

    [HttpGet("appointments")]
    public async Task<IActionResult> Appointments(
        [FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken ct)
        => Ok(await analyticsService.GetAppointmentReportAsync(from, to, ct));

    [HttpGet("revenue")]
    [RoleBasedAuth(UserRoles.Admin, UserRoles.AccountsManager)]
    public async Task<IActionResult> Revenue(CancellationToken ct)
        => Ok(await analyticsService.GetRevenueReportAsync(ct));

    [HttpGet("bed-occupancy")]
    public async Task<IActionResult> BedOccupancy(CancellationToken ct)
        => Ok(await analyticsService.GetBedOccupancyReportAsync(ct));

    [HttpGet("inventory")]
    public async Task<IActionResult> Inventory(CancellationToken ct)
        => Ok(await analyticsService.GetInventoryStatusReportAsync(ct));
}
