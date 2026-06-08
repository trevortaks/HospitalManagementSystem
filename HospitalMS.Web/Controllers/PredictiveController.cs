using System.Net.Http.Headers;
using HospitalMS.Business.Models;
using HospitalMS.Web.Filters;
using Microsoft.AspNetCore.Mvc;

namespace HospitalMS.Web.Controllers;

[Route("predictive")]
[RequireSession]
public sealed class PredictiveController(IHttpClientFactory httpClientFactory) : Controller
{
    private const string TokenSessionKey = "jwt_token";

    [HttpGet("")]
    public async Task<IActionResult> Summary(CancellationToken ct)
    {
        var client  = CreateAuthorizedClient();
        var summary = await client.GetFromJsonAsync<PredictiveInsightsSummary>("/api/predictive/summary", ct);

        ViewData["Title"]      = "Predictive Insights";
        ViewData["ActivePage"] = "Predictive";
        return View(summary);
    }

    [HttpGet("readmission")]
    public async Task<IActionResult> Readmission(CancellationToken ct)
    {
        var client = CreateAuthorizedClient();
        var risks  = await client.GetFromJsonAsync<List<ReadmissionRiskScore>>("/api/predictive/readmission-risks?topN=50", ct)
                     ?? [];

        ViewData["Title"]      = "Readmission Risk";
        ViewData["ActivePage"] = "Predictive";
        return View(risks);
    }

    [HttpGet("stock")]
    public async Task<IActionResult> Stock(CancellationToken ct)
    {
        var client      = CreateAuthorizedClient();
        var predictions = await client.GetFromJsonAsync<List<LowStockPrediction>>("/api/predictive/low-stock-predictions", ct)
                          ?? [];

        ViewData["Title"]      = "Stock Predictions";
        ViewData["ActivePage"] = "Predictive";
        return View(predictions);
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
