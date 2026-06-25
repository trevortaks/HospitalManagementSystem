using HospitalMS.Business.Models;
using HospitalMS.Web.Filters;
using Microsoft.AspNetCore.Mvc;

namespace HospitalMS.Web.Controllers;

[Route("lab")]
[RequireSession]
public sealed class LabController(IHttpClientFactory f) : AppController(f)
{

    // ── Panels ───────────────────────────────────────────────────────────────

    [HttpGet("panels")]
    public async Task<IActionResult> Panels(CancellationToken cancellationToken)
    {
        var client = Api();
        var panels = await client.GetFromJsonAsync<IReadOnlyList<LabOrderPanelResponse>>(
            "/api/lab-panels", cancellationToken) ?? [];
        return View(panels);
    }

    [HttpGet("panels/create")]
    public IActionResult CreatePanel() =>
        View(new CreateLabOrderPanelRequest(string.Empty, string.Empty));

    [HttpPost("panels/create")]
    public async Task<IActionResult> CreatePanel(CreateLabOrderPanelRequest request, CancellationToken cancellationToken)
    {
        var client = Api();
        var response = await client.PostAsJsonAsync("/api/lab-panels", request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError(string.Empty, "Failed to create panel.");
            return View(request);
        }
        return RedirectToAction(nameof(Panels));
    }

    [HttpPost("panels/{id:guid}/toggle-active")]
    public async Task<IActionResult> TogglePanelActive(Guid id, CancellationToken cancellationToken)
    {
        var client = Api();
        await client.PatchAsync($"/api/lab-panels/{id}/toggle-active", null, cancellationToken);
        return RedirectToAction(nameof(Panels));
    }

    // ── Orders ───────────────────────────────────────────────────────────────

    [HttpGet("orders")]
    public async Task<IActionResult> Orders(
        [FromQuery] Guid? patientId,
        [FromQuery] Guid? encounterId,
        [FromQuery] string? status,
        CancellationToken cancellationToken)
    {
        var client = Api();
        var qs = BuildQueryString(
            ("patientId", patientId?.ToString()),
            ("encounterId", encounterId?.ToString()),
            ("status", status));
        var orders = await client.GetFromJsonAsync<IReadOnlyList<LabOrderResponse>>(
            $"/api/lab-orders{qs}", cancellationToken) ?? [];
        ViewBag.PatientId = patientId;
        ViewBag.StatusFilter = status;
        return View(orders);
    }

    [HttpGet("orders/create")]
    public async Task<IActionResult> CreateOrder(
        [FromQuery] Guid? encounterId,
        [FromQuery] Guid? patientId,
        CancellationToken cancellationToken)
    {
        var client = Api();
        var panels = await client.GetFromJsonAsync<IReadOnlyList<LabOrderPanelResponse>>(
            "/api/lab-panels?activeOnly=true", cancellationToken) ?? [];
        var users = await client.GetFromJsonAsync<IReadOnlyList<UserSummary>>(
            "/api/users", cancellationToken) ?? [];

        ViewBag.Panels = panels;
        ViewBag.Doctors = users.Where(u => u.Role is "Doctor" or "Administrator").ToList();
        ViewBag.PreselectedEncounterId = encounterId;
        ViewBag.PreselectedPatientId = patientId;

        return View(new CreateLabOrderRequest(
            encounterId ?? Guid.Empty,
            patientId ?? Guid.Empty,
            Guid.Empty, Guid.Empty));
    }

    [HttpPost("orders/create")]
    public async Task<IActionResult> CreateOrder(CreateLabOrderRequest request, CancellationToken cancellationToken)
    {
        var client = Api();
        var response = await client.PostAsJsonAsync("/api/lab-orders", request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError(string.Empty, "Failed to create lab order.");
            var panels = await client.GetFromJsonAsync<IReadOnlyList<LabOrderPanelResponse>>("/api/lab-panels?activeOnly=true", cancellationToken) ?? [];
            var users  = await client.GetFromJsonAsync<IReadOnlyList<UserSummary>>("/api/users", cancellationToken) ?? [];
            ViewBag.Panels  = panels;
            ViewBag.Doctors = users.Where(u => u.Role is "Doctor" or "Administrator").ToList();
            return View(request);
        }
        return RedirectToAction(nameof(Orders), new { patientId = request.PatientId });
    }

    [HttpGet("orders/{id:guid}")]
    public async Task<IActionResult> OrderDetail(Guid id, CancellationToken cancellationToken)
    {
        var client = Api();
        var order = await client.GetFromJsonAsync<LabOrderResponse>($"/api/lab-orders/{id}", cancellationToken);
        if (order is null) return NotFound();

        var users = await client.GetFromJsonAsync<IReadOnlyList<UserSummary>>("/api/users", cancellationToken) ?? [];
        ViewBag.LabTechnicians = users.Where(u => u.Role is "LabTechnician" or "Radiologist" or "Administrator").ToList();
        return View(order);
    }

    [HttpPost("orders/{id:guid}/collect")]
    public async Task<IActionResult> CollectOrder(Guid id, CancellationToken cancellationToken)
    {
        var client = Api();
        await client.PatchAsync($"/api/lab-orders/{id}/collect", null, cancellationToken);
        return RedirectToAction(nameof(OrderDetail), new { id });
    }

    [HttpPost("orders/{id:guid}/cancel")]
    public async Task<IActionResult> CancelOrder(Guid id, Guid? patientId, CancellationToken cancellationToken)
    {
        var client = Api();
        await client.PatchAsync($"/api/lab-orders/{id}/cancel", null, cancellationToken);
        return RedirectToAction(nameof(Orders), new { patientId });
    }

    [HttpPost("orders/{id:guid}/results")]
    public async Task<IActionResult> AddResult(Guid id, AddLabResultRequest request, CancellationToken cancellationToken)
    {
        var client = Api();
        await client.PostAsJsonAsync($"/api/lab-orders/{id}/results", request, cancellationToken);
        return RedirectToAction(nameof(OrderDetail), new { id });
    }


}
