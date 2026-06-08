using HospitalMS.Business.Services;
using HospitalMS.Common.Auth;
using HospitalMS.Common.Constants;
using Microsoft.AspNetCore.Mvc;

namespace HospitalMS.API.Controllers;

[ApiController]
[Route("api/predictive")]
[RoleBasedAuth(UserRoles.Admin, UserRoles.Doctor)]
public sealed class PredictiveController(IPredictiveAnalyticsService predictiveService) : ControllerBase
{
    [HttpGet("summary")]
    public async Task<IActionResult> Summary(CancellationToken ct)
        => Ok(await predictiveService.GetInsightsSummaryAsync(ct));

    [HttpGet("readmission-risks")]
    public async Task<IActionResult> ReadmissionRisks(
        [FromQuery] int topN = 20, CancellationToken ct = default)
        => Ok(await predictiveService.GetReadmissionRisksAsync(topN, ct));

    [HttpGet("bed-demand")]
    public async Task<IActionResult> BedDemand(
        [FromQuery] int weeks = 4, CancellationToken ct = default)
        => Ok(await predictiveService.GetBedDemandForecastAsync(weeks, ct));

    [HttpGet("low-stock-predictions")]
    public async Task<IActionResult> LowStockPredictions(CancellationToken ct)
        => Ok(await predictiveService.GetLowStockPredictionsAsync(ct));

    [HttpGet("no-show-risks")]
    public async Task<IActionResult> NoShowRisks(CancellationToken ct)
        => Ok(await predictiveService.GetNoShowRisksAsync(ct));
}
