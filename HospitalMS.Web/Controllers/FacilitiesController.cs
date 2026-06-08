using System.Net.Http.Headers;
using HospitalMS.Business.Models;
using HospitalMS.Web.Filters;
using Microsoft.AspNetCore.Mvc;

namespace HospitalMS.Web.Controllers;

[Route("facilities")]
[RequireSession]
public sealed class FacilitiesController(IHttpClientFactory httpClientFactory) : Controller
{
    private const string TokenSessionKey = "jwt_token";

    [HttpGet("rooms")]
    public async Task<IActionResult> Rooms(CancellationToken ct)
    {
        var client = CreateAuthorizedClient();
        var rooms = await client.GetFromJsonAsync<IReadOnlyList<RoomResponse>>(
            "/api/facilities/rooms", ct) ?? [];

        ViewData["Title"]      = "Rooms";
        ViewData["ActivePage"] = "Facilities";
        return View(rooms);
    }

    [HttpPost("rooms/create")]
    public async Task<IActionResult> CreateRoom(
        [FromForm] string Name, [FromForm] string RoomNumber, [FromForm] string RoomType,
        [FromForm] int FloorNumber, [FromForm] string? Building, [FromForm] int CapacityPersons,
        [FromForm] string? Notes, CancellationToken ct)
    {
        var request = new CreateRoomRequest(Name, RoomNumber, RoomType, FloorNumber, Building, CapacityPersons, Notes);
        var client = CreateAuthorizedClient();
        await client.PostAsJsonAsync("/api/facilities/rooms", request, ct);
        return RedirectToAction(nameof(Rooms));
    }

    [HttpPost("rooms/{id:guid}/toggle")]
    public async Task<IActionResult> ToggleRoom(Guid id, CancellationToken ct)
    {
        var client = CreateAuthorizedClient();
        await client.PatchAsync($"/api/facilities/rooms/{id}/toggle-active", null, ct);
        return RedirectToAction(nameof(Rooms));
    }

    [HttpGet("equipment")]
    public async Task<IActionResult> Equipment([FromQuery] string? status, CancellationToken ct)
    {
        var client = CreateAuthorizedClient();
        var url = "/api/facilities/equipment" + (string.IsNullOrWhiteSpace(status) ? "" : $"?status={status}");
        var equipment = await client.GetFromJsonAsync<IReadOnlyList<EquipmentResponse>>(url, ct) ?? [];
        var rooms     = await client.GetFromJsonAsync<IReadOnlyList<RoomResponse>>("/api/facilities/rooms?activeOnly=true", ct) ?? [];

        ViewBag.Status = status;
        ViewBag.Rooms  = rooms;
        ViewData["Title"]      = "Equipment";
        ViewData["ActivePage"] = "Facilities";
        return View(equipment);
    }

    [HttpPost("equipment/create")]
    public async Task<IActionResult> CreateEquipment(
        [FromForm] string Name, [FromForm] string Code, [FromForm] string EquipmentType,
        [FromForm] string? SerialNumber, [FromForm] string? Manufacturer, [FromForm] string? Model,
        [FromForm] Guid? LocationRoomId, [FromForm] string? Notes, CancellationToken ct)
    {
        var request = new CreateEquipmentRequest(Name, Code, EquipmentType, SerialNumber,
            Manufacturer, Model, LocationRoomId, Notes: Notes);
        var client = CreateAuthorizedClient();
        await client.PostAsJsonAsync("/api/facilities/equipment", request, ct);
        return RedirectToAction(nameof(Equipment));
    }

    [HttpPost("equipment/{id:guid}/status")]
    public async Task<IActionResult> UpdateEquipmentStatus(Guid id, [FromForm] string Status, CancellationToken ct)
    {
        var client = CreateAuthorizedClient();
        await client.PatchAsJsonAsync($"/api/facilities/equipment/{id}/status",
            new UpdateEquipmentStatusRequest(Status), ct);
        return RedirectToAction(nameof(Equipment));
    }

    [HttpGet("maintenance")]
    public async Task<IActionResult> MaintenanceRequests([FromQuery] string? status, CancellationToken ct)
    {
        var client = CreateAuthorizedClient();
        var url = "/api/facilities/maintenance-requests" + (string.IsNullOrWhiteSpace(status) ? "" : $"?status={status}");
        var requests = await client.GetFromJsonAsync<IReadOnlyList<MaintenanceRequestResponse>>(url, ct) ?? [];
        var rooms    = await client.GetFromJsonAsync<IReadOnlyList<RoomResponse>>("/api/facilities/rooms?activeOnly=true", ct) ?? [];
        var equipment = await client.GetFromJsonAsync<IReadOnlyList<EquipmentResponse>>("/api/facilities/equipment", ct) ?? [];
        var users    = await client.GetFromJsonAsync<IReadOnlyList<UserSummary>>("/api/users", ct) ?? [];

        ViewBag.Status    = status;
        ViewBag.Rooms     = rooms;
        ViewBag.Equipment = equipment;
        ViewBag.Staff     = users;
        ViewData["Title"]      = "Maintenance Requests";
        ViewData["ActivePage"] = "Facilities";
        return View(requests);
    }

    [HttpPost("maintenance/create")]
    public async Task<IActionResult> CreateMaintenanceRequest(
        [FromForm] string Title, [FromForm] string Description, [FromForm] Guid RequestedByUserId,
        [FromForm] string RequestType, [FromForm] string Priority,
        [FromForm] Guid? RoomId, [FromForm] Guid? EquipmentId, CancellationToken ct)
    {
        var request = new CreateMaintenanceRequestRequest(
            Title, Description, RequestedByUserId, RequestType, Priority, RoomId, EquipmentId);
        var client = CreateAuthorizedClient();
        await client.PostAsJsonAsync("/api/facilities/maintenance-requests", request, ct);
        return RedirectToAction(nameof(MaintenanceRequests));
    }

    [HttpPost("maintenance/{id:guid}/assign")]
    public async Task<IActionResult> AssignRequest(Guid id, [FromForm] Guid AssignedToUserId, CancellationToken ct)
    {
        var client = CreateAuthorizedClient();
        await client.PatchAsJsonAsync($"/api/facilities/maintenance-requests/{id}/assign",
            new AssignMaintenanceRequestRequest(AssignedToUserId), ct);
        return RedirectToAction(nameof(MaintenanceRequests));
    }

    [HttpPost("maintenance/{id:guid}/resolve")]
    public async Task<IActionResult> ResolveRequest(Guid id, [FromForm] string ResolutionNotes, CancellationToken ct)
    {
        var client = CreateAuthorizedClient();
        await client.PatchAsJsonAsync($"/api/facilities/maintenance-requests/{id}/resolve",
            new ResolveMaintenanceRequestRequest(ResolutionNotes), ct);
        return RedirectToAction(nameof(MaintenanceRequests));
    }

    [HttpPost("maintenance/{id:guid}/close")]
    public async Task<IActionResult> CloseRequest(Guid id, CancellationToken ct)
    {
        var client = CreateAuthorizedClient();
        await client.PatchAsync($"/api/facilities/maintenance-requests/{id}/close", null, ct);
        return RedirectToAction(nameof(MaintenanceRequests));
    }

    private HttpClient CreateAuthorizedClient()
    {
        var client = httpClientFactory.CreateClient("HospitalAPI");
        var token = HttpContext.Session.GetString(TokenSessionKey);
        if (!string.IsNullOrEmpty(token))
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }
}
