using HospitalMS.Data.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HospitalMS.API.Controllers;

[ApiController]
[Route("diagnostics")]
public sealed class DiagnosticsController(HospitalDbContext dbContext, IHttpClientFactory httpClientFactory) : ControllerBase
{
    [HttpGet("database")]
    public async Task<IActionResult> Database(CancellationToken cancellationToken)
    {
        var canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);
        if (!canConnect)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new
            {
                status = "Unavailable",
                database = dbContext.Database.GetDbConnection().Database
            });
        }

        return Ok(new
        {
            status = "Healthy",
            database = dbContext.Database.GetDbConnection().Database,
            provider = dbContext.Database.ProviderName
        });
    }

    [HttpGet("service-discovery")]
    public async Task<IActionResult> ServiceDiscovery(CancellationToken cancellationToken)
    {
        var client = httpClientFactory.CreateClient();
        using var response = await client.GetAsync("http://hospitalms-web/", cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new
            {
                status = "Unavailable",
                target = "hospitalms-web",
                response.StatusCode
            });
        }

        return Ok(new
        {
            status = "Healthy",
            target = "hospitalms-web",
            containsWelcomeText = responseBody.Contains("Welcome", StringComparison.OrdinalIgnoreCase)
        });
    }
}
