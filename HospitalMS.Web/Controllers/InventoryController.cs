using System.Net.Http.Headers;
using HospitalMS.Business.Models;
using HospitalMS.Web.Filters;
using Microsoft.AspNetCore.Mvc;

namespace HospitalMS.Web.Controllers;

[Route("inventory")]
[RequireSession]
public sealed class InventoryController(IHttpClientFactory httpClientFactory) : Controller
{
    private const string TokenSessionKey = "jwt_token";

    [HttpGet("categories")]
    public async Task<IActionResult> Categories(CancellationToken cancellationToken)
    {
        var client = CreateAuthorizedClient();
        var cats = await client.GetFromJsonAsync<IReadOnlyList<InventoryCategoryResponse>>(
            "/api/inventory/categories", cancellationToken) ?? [];

        ViewData["Title"] = "Inventory Categories";
        ViewData["ActivePage"] = "Inventory";
        return View(cats);
    }

    [HttpGet("categories/create")]
    public IActionResult CreateCategory()
    {
        ViewData["Title"] = "Create Category";
        ViewData["ActivePage"] = "Inventory";
        return View();
    }

    [HttpPost("categories/create")]
    public async Task<IActionResult> CreateCategory(CreateInventoryCategoryRequest request, CancellationToken cancellationToken)
    {
        var client = CreateAuthorizedClient();
        var response = await client.PostAsJsonAsync("/api/inventory/categories", request, cancellationToken);
        if (response.IsSuccessStatusCode) return RedirectToAction(nameof(Categories));

        ViewData["Error"] = await response.Content.ReadAsStringAsync(cancellationToken);
        ViewData["Title"] = "Create Category";
        ViewData["ActivePage"] = "Inventory";
        return View(request);
    }

    [HttpPost("categories/{id:guid}/toggle")]
    public async Task<IActionResult> ToggleCategory(Guid id, CancellationToken cancellationToken)
    {
        var client = CreateAuthorizedClient();
        await client.PatchAsync($"/api/inventory/categories/{id}/toggle-active", null, cancellationToken);
        return RedirectToAction(nameof(Categories));
    }

    [HttpGet("items")]
    public async Task<IActionResult> Items([FromQuery] Guid? categoryId, [FromQuery] bool? lowStockOnly, CancellationToken cancellationToken)
    {
        var client = CreateAuthorizedClient();

        var qs = new List<string>();
        if (categoryId.HasValue) qs.Add($"categoryId={categoryId}");
        if (lowStockOnly == true) qs.Add("lowStockOnly=true");
        var url = "/api/inventory/items" + (qs.Any() ? "?" + string.Join("&", qs) : "");

        var items = await client.GetFromJsonAsync<IReadOnlyList<InventoryItemResponse>>(url, cancellationToken) ?? [];
        var cats = await client.GetFromJsonAsync<IReadOnlyList<InventoryCategoryResponse>>(
            "/api/inventory/categories?activeOnly=true", cancellationToken) ?? [];

        ViewBag.Categories = cats;
        ViewBag.CategoryId = categoryId;
        ViewBag.LowStockOnly = lowStockOnly;
        ViewData["Title"] = "Inventory Items";
        ViewData["ActivePage"] = "Inventory";
        return View(items);
    }

    [HttpGet("items/{id:guid}")]
    public async Task<IActionResult> ItemDetail(Guid id, CancellationToken cancellationToken)
    {
        var client = CreateAuthorizedClient();
        var itemResponse = await client.GetAsync($"/api/inventory/items/{id}", cancellationToken);
        if (!itemResponse.IsSuccessStatusCode) return NotFound();
        var item = await itemResponse.Content.ReadFromJsonAsync<InventoryItemResponse>(cancellationToken);
        if (item is null) return NotFound();

        var txs = await client.GetFromJsonAsync<IReadOnlyList<StockTransactionResponse>>(
            $"/api/inventory/items/{id}/transactions", cancellationToken) ?? [];

        var usersResponse = await client.GetAsync("/api/users", cancellationToken);
        IReadOnlyList<UserSummary> staff = [];
        if (usersResponse.IsSuccessStatusCode)
            staff = await usersResponse.Content.ReadFromJsonAsync<IReadOnlyList<UserSummary>>(cancellationToken) ?? [];

        ViewBag.Transactions = txs;
        ViewBag.Staff = staff;
        ViewData["Title"] = item.Name;
        ViewData["ActivePage"] = "Inventory";
        return View(item);
    }

    [HttpGet("items/create")]
    public async Task<IActionResult> CreateItem(CancellationToken cancellationToken)
    {
        var client = CreateAuthorizedClient();
        var cats = await client.GetFromJsonAsync<IReadOnlyList<InventoryCategoryResponse>>(
            "/api/inventory/categories?activeOnly=true", cancellationToken) ?? [];

        ViewBag.Categories = cats;
        ViewData["Title"] = "Create Item";
        ViewData["ActivePage"] = "Inventory";
        return View();
    }

    [HttpPost("items/create")]
    public async Task<IActionResult> CreateItem(CreateInventoryItemRequest request, CancellationToken cancellationToken)
    {
        var client = CreateAuthorizedClient();
        var response = await client.PostAsJsonAsync("/api/inventory/items", request, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            var item = await response.Content.ReadFromJsonAsync<InventoryItemResponse>(cancellationToken);
            return RedirectToAction(nameof(ItemDetail), new { id = item!.Id });
        }

        var cats = await client.GetFromJsonAsync<IReadOnlyList<InventoryCategoryResponse>>(
            "/api/inventory/categories?activeOnly=true", cancellationToken) ?? [];
        ViewBag.Categories = cats;
        ViewData["Error"] = await response.Content.ReadAsStringAsync(cancellationToken);
        ViewData["Title"] = "Create Item";
        ViewData["ActivePage"] = "Inventory";
        return View(request);
    }

    [HttpPost("items/{id:guid}/stock-in")]
    public async Task<IActionResult> StockIn(Guid id, StockInRequest request, CancellationToken cancellationToken)
    {
        var client = CreateAuthorizedClient();
        await client.PostAsJsonAsync("/api/inventory/items/stock-in", request with { ItemId = id }, cancellationToken);
        return RedirectToAction(nameof(ItemDetail), new { id });
    }

    [HttpPost("items/{id:guid}/stock-out")]
    public async Task<IActionResult> StockOut(Guid id, StockOutRequest request, CancellationToken cancellationToken)
    {
        var client = CreateAuthorizedClient();
        await client.PostAsJsonAsync("/api/inventory/items/stock-out", request with { ItemId = id }, cancellationToken);
        return RedirectToAction(nameof(ItemDetail), new { id });
    }

    [HttpPost("items/{id:guid}/adjust")]
    public async Task<IActionResult> AdjustStock(Guid id, AdjustStockRequest request, CancellationToken cancellationToken)
    {
        var client = CreateAuthorizedClient();
        await client.PostAsJsonAsync("/api/inventory/items/adjust", request with { ItemId = id }, cancellationToken);
        return RedirectToAction(nameof(ItemDetail), new { id });
    }

    [HttpPost("items/{id:guid}/toggle")]
    public async Task<IActionResult> ToggleItem(Guid id, CancellationToken cancellationToken)
    {
        var client = CreateAuthorizedClient();
        await client.PatchAsync($"/api/inventory/items/{id}/toggle-active", null, cancellationToken);
        return RedirectToAction(nameof(ItemDetail), new { id });
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
