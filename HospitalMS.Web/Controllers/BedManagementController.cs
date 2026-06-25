using HospitalMS.Business.Models;
using HospitalMS.Web.Filters;
using Microsoft.AspNetCore.Mvc;

namespace HospitalMS.Web.Controllers;

[Route("beds")]
[RequireSession]
public sealed class BedManagementController(IHttpClientFactory f) : AppController(f)
{

    // ── Wards ─────────────────────────────────────────────────────────────────

    [HttpGet("wards")]
    public async Task<IActionResult> Wards(CancellationToken cancellationToken)
    {
        var client = Api();
        var wards = await client.GetFromJsonAsync<IReadOnlyList<WardResponse>>("/api/wards", cancellationToken) ?? [];

        ViewData["Title"] = "Wards";
        ViewData["ActivePage"] = "Beds";
        return View(wards);
    }

    [HttpGet("wards/create")]
    public IActionResult CreateWard()
    {
        ViewData["Title"] = "Create Ward";
        ViewData["ActivePage"] = "Beds";
        return View();
    }

    [HttpPost("wards/create")]
    public async Task<IActionResult> CreateWard(CreateWardRequest request, CancellationToken cancellationToken)
    {
        var client = Api();
        var response = await client.PostAsJsonAsync("/api/wards", request, cancellationToken);
        if (response.IsSuccessStatusCode) return RedirectToAction(nameof(Wards));

        ViewData["Error"] = await response.Content.ReadAsStringAsync(cancellationToken);
        ViewData["Title"] = "Create Ward";
        ViewData["ActivePage"] = "Beds";
        return View(request);
    }

    [HttpPost("wards/{id:guid}/toggle")]
    public async Task<IActionResult> ToggleWard(Guid id, CancellationToken cancellationToken)
    {
        var client = Api();
        await client.PatchAsync($"/api/wards/{id}/toggle-active", null, cancellationToken);
        return RedirectToAction(nameof(Wards));
    }

    // ── Beds ─────────────────────────────────────────────────────────────────

    [HttpGet("wards/{wardId:guid}")]
    public async Task<IActionResult> WardDetail(Guid wardId, CancellationToken cancellationToken)
    {
        var client = Api();

        var wardResponse = await client.GetAsync($"/api/wards/{wardId}", cancellationToken);
        if (!wardResponse.IsSuccessStatusCode) return NotFound();
        var ward = await wardResponse.Content.ReadFromJsonAsync<WardResponse>(cancellationToken);
        if (ward is null) return NotFound();

        var beds = await client.GetFromJsonAsync<IReadOnlyList<BedResponse>>(
            $"/api/wards/{wardId}/beds", cancellationToken) ?? [];

        var allocationsResponse = await client.GetAsync(
            $"/api/beds/allocations?wardId={wardId}", cancellationToken);
        IReadOnlyList<BedAllocationResponse> allocations = [];
        if (allocationsResponse.IsSuccessStatusCode)
            allocations = await allocationsResponse.Content
                .ReadFromJsonAsync<IReadOnlyList<BedAllocationResponse>>(cancellationToken) ?? [];

        ViewBag.Ward = ward;
        ViewBag.Allocations = allocations;
        ViewData["Title"] = $"{ward.Name} — Beds";
        ViewData["ActivePage"] = "Beds";
        return View(beds);
    }

    [HttpGet("wards/{wardId:guid}/beds/create")]
    public async Task<IActionResult> CreateBed(Guid wardId, CancellationToken cancellationToken)
    {
        var client = Api();
        var wardResponse = await client.GetAsync($"/api/wards/{wardId}", cancellationToken);
        if (!wardResponse.IsSuccessStatusCode) return NotFound();
        var ward = await wardResponse.Content.ReadFromJsonAsync<WardResponse>(cancellationToken);

        ViewBag.Ward = ward;
        ViewData["Title"] = "Add Bed";
        ViewData["ActivePage"] = "Beds";
        return View(new CreateBedRequest(wardId, string.Empty));
    }

    [HttpPost("wards/{wardId:guid}/beds/create")]
    public async Task<IActionResult> CreateBed(Guid wardId, CreateBedRequest request, CancellationToken cancellationToken)
    {
        var client = Api();
        var response = await client.PostAsJsonAsync($"/api/wards/{wardId}/beds", request with { WardId = wardId }, cancellationToken);
        if (response.IsSuccessStatusCode) return RedirectToAction(nameof(WardDetail), new { wardId });

        ViewData["Error"] = await response.Content.ReadAsStringAsync(cancellationToken);
        ViewData["Title"] = "Add Bed";
        ViewData["ActivePage"] = "Beds";
        return View(request);
    }

    [HttpPost("beds/{id:guid}/toggle")]
    public async Task<IActionResult> ToggleBed(Guid id, Guid wardId, CancellationToken cancellationToken)
    {
        var client = Api();
        await client.PatchAsync($"/api/beds/{id}/toggle-active", null, cancellationToken);
        return RedirectToAction(nameof(WardDetail), new { wardId });
    }

    // ── Allocations ───────────────────────────────────────────────────────────

    [HttpGet("allocate/{bedId:guid}")]
    public async Task<IActionResult> Allocate(Guid bedId, CancellationToken cancellationToken)
    {
        var client = Api();
        var bedResponse = await client.GetAsync($"/api/beds/{bedId}", cancellationToken);
        if (!bedResponse.IsSuccessStatusCode) return NotFound();
        var bed = await bedResponse.Content.ReadFromJsonAsync<BedResponse>(cancellationToken);

        var usersResponse = await client.GetAsync("/api/users", cancellationToken);
        IReadOnlyList<UserSummary> staff = [];
        if (usersResponse.IsSuccessStatusCode)
            staff = await usersResponse.Content.ReadFromJsonAsync<IReadOnlyList<UserSummary>>(cancellationToken) ?? [];

        ViewBag.Bed = bed;
        ViewBag.Staff = staff;
        ViewData["Title"] = "Allocate Bed";
        ViewData["ActivePage"] = "Beds";
        return View();
    }

    [HttpPost("allocate/{bedId:guid}")]
    public async Task<IActionResult> Allocate(Guid bedId, AllocateBedRequest request, CancellationToken cancellationToken)
    {
        var client = Api();
        var response = await client.PostAsJsonAsync("/api/beds/allocate", request with { BedId = bedId }, cancellationToken);
        if (response.IsSuccessStatusCode) return RedirectToAction(nameof(WardDetail), new { wardId = (await response.Content.ReadFromJsonAsync<BedAllocationResponse>(cancellationToken))?.BedId });

        ViewData["Error"] = await response.Content.ReadAsStringAsync(cancellationToken);
        ViewData["Title"] = "Allocate Bed";
        ViewData["ActivePage"] = "Beds";
        return View();
    }

    [HttpPost("discharge/{allocationId:guid}")]
    public async Task<IActionResult> Discharge(Guid allocationId, DischargePatientRequest request, CancellationToken cancellationToken)
    {
        var client = Api();
        await client.PostAsJsonAsync("/api/beds/discharge", request with { AllocationId = allocationId }, cancellationToken);
        return RedirectToAction(nameof(Wards));
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

}
