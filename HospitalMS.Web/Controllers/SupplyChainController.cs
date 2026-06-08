using System.Net.Http.Headers;
using HospitalMS.Business.Models;
using HospitalMS.Web.Filters;
using Microsoft.AspNetCore.Mvc;

namespace HospitalMS.Web.Controllers;

[Route("supply-chain")]
[RequireSession]
public sealed class SupplyChainController(IHttpClientFactory httpClientFactory) : Controller
{
    private const string TokenSessionKey = "jwt_token";

    // ── Suppliers ─────────────────────────────────────────────────────────────

    [HttpGet("suppliers")]
    public async Task<IActionResult> Suppliers(CancellationToken cancellationToken)
    {
        var client = CreateAuthorizedClient();
        var suppliers = await client.GetFromJsonAsync<IReadOnlyList<SupplierResponse>>(
            "/api/suppliers", cancellationToken) ?? [];

        ViewData["Title"] = "Suppliers";
        ViewData["ActivePage"] = "SupplyChain";
        return View(suppliers);
    }

    [HttpGet("suppliers/create")]
    public IActionResult CreateSupplier()
    {
        ViewData["Title"] = "Add Supplier";
        ViewData["ActivePage"] = "SupplyChain";
        return View();
    }

    [HttpPost("suppliers/create")]
    public async Task<IActionResult> CreateSupplier(CreateSupplierRequest request, CancellationToken cancellationToken)
    {
        var client = CreateAuthorizedClient();
        var response = await client.PostAsJsonAsync("/api/suppliers", request, cancellationToken);
        if (response.IsSuccessStatusCode) return RedirectToAction(nameof(Suppliers));

        ViewData["Error"] = await response.Content.ReadAsStringAsync(cancellationToken);
        ViewData["Title"] = "Add Supplier";
        ViewData["ActivePage"] = "SupplyChain";
        return View(request);
    }

    [HttpPost("suppliers/{id:guid}/toggle")]
    public async Task<IActionResult> ToggleSupplier(Guid id, CancellationToken cancellationToken)
    {
        var client = CreateAuthorizedClient();
        await client.PatchAsync($"/api/suppliers/{id}/toggle-active", null, cancellationToken);
        return RedirectToAction(nameof(Suppliers));
    }

    // ── Purchase Orders ───────────────────────────────────────────────────────

    [HttpGet("purchase-orders")]
    public async Task<IActionResult> PurchaseOrders([FromQuery] string? status, CancellationToken cancellationToken)
    {
        var client = CreateAuthorizedClient();
        var url = "/api/purchase-orders" + (string.IsNullOrWhiteSpace(status) ? "" : $"?status={status}");
        var orders = await client.GetFromJsonAsync<IReadOnlyList<PurchaseOrderResponse>>(url, cancellationToken) ?? [];

        ViewBag.Status = status;
        ViewData["Title"] = "Purchase Orders";
        ViewData["ActivePage"] = "SupplyChain";
        return View(orders);
    }

    [HttpGet("purchase-orders/create")]
    public async Task<IActionResult> CreatePurchaseOrder(CancellationToken cancellationToken)
    {
        var client = CreateAuthorizedClient();
        var suppliers = await client.GetFromJsonAsync<IReadOnlyList<SupplierResponse>>(
            "/api/suppliers?activeOnly=true", cancellationToken) ?? [];
        var items = await client.GetFromJsonAsync<IReadOnlyList<InventoryItemResponse>>(
            "/api/inventory/items", cancellationToken) ?? [];
        var usersResponse = await client.GetAsync("/api/users", cancellationToken);
        IReadOnlyList<UserSummary> staff = [];
        if (usersResponse.IsSuccessStatusCode)
            staff = await usersResponse.Content.ReadFromJsonAsync<IReadOnlyList<UserSummary>>(cancellationToken) ?? [];

        ViewBag.Suppliers = suppliers;
        ViewBag.Items = items;
        ViewBag.Staff = staff;
        ViewData["Title"] = "Create Purchase Order";
        ViewData["ActivePage"] = "SupplyChain";
        return View();
    }

    [HttpPost("purchase-orders/create")]
    public async Task<IActionResult> CreatePurchaseOrder(
        [FromForm] Guid SupplierId,
        [FromForm] Guid OrderedByUserId,
        [FromForm] string? Notes,
        [FromForm] DateTime? ExpectedDeliveryDate,
        [FromForm] List<Guid> ItemIds,
        [FromForm] List<int> Quantities,
        [FromForm] List<decimal> UnitCosts,
        CancellationToken cancellationToken)
    {
        var lines = ItemIds
            .Select((id, i) => new CreatePurchaseOrderLineRequest(id, Quantities[i], UnitCosts[i]))
            .Where(l => l.QuantityOrdered > 0)
            .ToList();

        var request = new CreatePurchaseOrderRequest(SupplierId, OrderedByUserId, lines, Notes, ExpectedDeliveryDate);
        var client = CreateAuthorizedClient();
        var response = await client.PostAsJsonAsync("/api/purchase-orders", request, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var po = await response.Content.ReadFromJsonAsync<PurchaseOrderResponse>(cancellationToken);
            return RedirectToAction(nameof(PurchaseOrderDetail), new { id = po!.Id });
        }

        ViewData["Error"] = await response.Content.ReadAsStringAsync(cancellationToken);
        return RedirectToAction(nameof(CreatePurchaseOrder));
    }

    [HttpGet("purchase-orders/{id:guid}")]
    public async Task<IActionResult> PurchaseOrderDetail(Guid id, CancellationToken cancellationToken)
    {
        var client = CreateAuthorizedClient();
        var poResponse = await client.GetAsync($"/api/purchase-orders/{id}", cancellationToken);
        if (!poResponse.IsSuccessStatusCode) return NotFound();
        var po = await poResponse.Content.ReadFromJsonAsync<PurchaseOrderResponse>(cancellationToken);
        if (po is null) return NotFound();

        var receipts = await client.GetFromJsonAsync<IReadOnlyList<GoodsReceiptResponse>>(
            $"/api/purchase-orders/{id}/receipts", cancellationToken) ?? [];

        var usersResponse = await client.GetAsync("/api/users", cancellationToken);
        IReadOnlyList<UserSummary> staff = [];
        if (usersResponse.IsSuccessStatusCode)
            staff = await usersResponse.Content.ReadFromJsonAsync<IReadOnlyList<UserSummary>>(cancellationToken) ?? [];

        ViewBag.Receipts = receipts;
        ViewBag.Staff = staff;
        ViewData["Title"] = po.OrderNumber;
        ViewData["ActivePage"] = "SupplyChain";
        return View(po);
    }

    [HttpPost("purchase-orders/{id:guid}/submit")]
    public async Task<IActionResult> SubmitOrder(Guid id, CancellationToken cancellationToken)
    {
        var client = CreateAuthorizedClient();
        await client.PatchAsync($"/api/purchase-orders/{id}/submit", null, cancellationToken);
        return RedirectToAction(nameof(PurchaseOrderDetail), new { id });
    }

    [HttpPost("purchase-orders/{id:guid}/approve")]
    public async Task<IActionResult> ApproveOrder(Guid id, CancellationToken cancellationToken)
    {
        var client = CreateAuthorizedClient();
        await client.PatchAsync($"/api/purchase-orders/{id}/approve", null, cancellationToken);
        return RedirectToAction(nameof(PurchaseOrderDetail), new { id });
    }

    [HttpPost("purchase-orders/{id:guid}/cancel")]
    public async Task<IActionResult> CancelOrder(Guid id, CancellationToken cancellationToken)
    {
        var client = CreateAuthorizedClient();
        await client.PatchAsync($"/api/purchase-orders/{id}/cancel", null, cancellationToken);
        return RedirectToAction(nameof(PurchaseOrderDetail), new { id });
    }

    [HttpPost("purchase-orders/{id:guid}/receive")]
    public async Task<IActionResult> ReceiveGoods(
        Guid id,
        [FromForm] Guid ReceivedByUserId,
        [FromForm] string? Notes,
        [FromForm] List<Guid> LineIds,
        [FromForm] List<int> LineQuantities,
        CancellationToken cancellationToken)
    {
        var lines = LineIds
            .Select((lineId, i) => new ReceivePOLineRequest(lineId, LineQuantities[i]))
            .Where(l => l.QuantityReceived > 0)
            .ToList();

        var request = new ReceiveGoodsRequest(id, ReceivedByUserId, lines, Notes);
        var client = CreateAuthorizedClient();
        await client.PostAsJsonAsync($"/api/purchase-orders/{id}/receive", request, cancellationToken);
        return RedirectToAction(nameof(PurchaseOrderDetail), new { id });
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
