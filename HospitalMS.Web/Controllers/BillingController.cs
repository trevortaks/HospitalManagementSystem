using System.Net.Http.Headers;
using HospitalMS.Business.Models;
using HospitalMS.Web.Filters;
using Microsoft.AspNetCore.Mvc;

namespace HospitalMS.Web.Controllers;

[Route("billing")]
[RequireSession]
public sealed class BillingController(IHttpClientFactory httpClientFactory) : Controller
{
    private const string TokenSessionKey = "jwt_token";

    // ── Invoices ─────────────────────────────────────────────────────────────

    [HttpGet("invoices")]
    public async Task<IActionResult> Invoices([FromQuery] Guid? patientId, [FromQuery] string? status, CancellationToken cancellationToken)
    {
        var client = CreateAuthorizedClient();
        var url = "/api/invoices";
        var qs = new List<string>();
        if (patientId.HasValue) qs.Add($"patientId={patientId}");
        if (!string.IsNullOrWhiteSpace(status)) qs.Add($"status={status}");
        if (qs.Any()) url += "?" + string.Join("&", qs);

        var response = await client.GetAsync(url, cancellationToken);
        IReadOnlyList<InvoiceResponse> invoices = [];
        if (response.IsSuccessStatusCode)
            invoices = await response.Content.ReadFromJsonAsync<IReadOnlyList<InvoiceResponse>>(cancellationToken) ?? [];

        ViewBag.PatientId = patientId;
        ViewBag.Status = status;
        ViewData["Title"] = "Invoices";
        ViewData["ActivePage"] = "Billing";
        return View(invoices);
    }

    [HttpGet("invoices/{id:guid}")]
    public async Task<IActionResult> InvoiceDetail(Guid id, CancellationToken cancellationToken)
    {
        var client = CreateAuthorizedClient();
        var response = await client.GetAsync($"/api/invoices/{id}", cancellationToken);
        if (!response.IsSuccessStatusCode) return NotFound();
        var invoice = await response.Content.ReadFromJsonAsync<InvoiceResponse>(cancellationToken);
        if (invoice is null) return NotFound();

        var chargeItems = await client.GetFromJsonAsync<IReadOnlyList<ChargeItemResponse>>(
            "/api/charge-items?activeOnly=true", cancellationToken) ?? [];

        var usersResponse = await client.GetAsync("/api/users", cancellationToken);
        IReadOnlyList<UserSummary> accountsStaff = [];
        if (usersResponse.IsSuccessStatusCode)
        {
            var all = await usersResponse.Content.ReadFromJsonAsync<IReadOnlyList<UserSummary>>(cancellationToken) ?? [];
            accountsStaff = [.. all.Where(u => u.Role is "AccountsManager" or "Admin")];
        }

        var claimsResponse = await client.GetAsync($"/api/insurance/claims/{id}", cancellationToken);
        IReadOnlyList<InsuranceClaimResponse> claims = [];
        if (claimsResponse.IsSuccessStatusCode)
            claims = await claimsResponse.Content.ReadFromJsonAsync<IReadOnlyList<InsuranceClaimResponse>>(cancellationToken) ?? [];

        ViewBag.ChargeItems = chargeItems;
        ViewBag.AccountsStaff = accountsStaff;
        ViewBag.Claims = claims;
        ViewData["Title"] = $"Invoice {invoice.InvoiceNumber}";
        ViewData["ActivePage"] = "Billing";
        return View(invoice);
    }

    [HttpGet("invoices/create")]
    public async Task<IActionResult> CreateInvoice([FromQuery] Guid? patientId, [FromQuery] Guid? encounterId, CancellationToken cancellationToken)
    {
        var client = CreateAuthorizedClient();
        var usersResponse = await client.GetAsync("/api/users", cancellationToken);
        IReadOnlyList<UserSummary> staff = [];
        if (usersResponse.IsSuccessStatusCode)
        {
            var all = await usersResponse.Content.ReadFromJsonAsync<IReadOnlyList<UserSummary>>(cancellationToken) ?? [];
            staff = [.. all.Where(u => u.Role is "AccountsManager" or "Admin" or "Receptionist")];
        }

        ViewBag.Staff = staff;
        ViewBag.PreselectedPatientId = patientId;
        ViewBag.PreselectedEncounterId = encounterId;
        ViewData["Title"] = "New Invoice";
        ViewData["ActivePage"] = "Billing";
        return View();
    }

    [HttpPost("invoices/create")]
    public async Task<IActionResult> CreateInvoice(CreateInvoiceRequest request, CancellationToken cancellationToken)
    {
        var client = CreateAuthorizedClient();
        var response = await client.PostAsJsonAsync("/api/invoices", request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            TempData["Error"] = "Failed to create invoice.";
            return RedirectToAction(nameof(Invoices));
        }
        var invoice = await response.Content.ReadFromJsonAsync<InvoiceResponse>(cancellationToken);
        TempData["SuccessMessage"] = "Invoice created.";
        return RedirectToAction(nameof(InvoiceDetail), new { id = invoice!.Id });
    }

    [HttpPost("invoices/{id:guid}/line-items")]
    public async Task<IActionResult> AddLineItem(Guid id, [FromForm] Guid chargeItemId, [FromForm] int quantity, CancellationToken cancellationToken)
    {
        var client = CreateAuthorizedClient();
        await client.PostAsJsonAsync($"/api/invoices/{id}/line-items", new AddLineItemRequest(chargeItemId, quantity), cancellationToken);
        return RedirectToAction(nameof(InvoiceDetail), new { id });
    }

    [HttpPost("invoices/{id:guid}/line-items/{lineItemId:guid}/remove")]
    public async Task<IActionResult> RemoveLineItem(Guid id, Guid lineItemId, CancellationToken cancellationToken)
    {
        var client = CreateAuthorizedClient();
        await client.DeleteAsync($"/api/invoices/{id}/line-items/{lineItemId}", cancellationToken);
        return RedirectToAction(nameof(InvoiceDetail), new { id });
    }

    [HttpPost("invoices/{id:guid}/issue")]
    public async Task<IActionResult> IssueInvoice(Guid id, CancellationToken cancellationToken)
    {
        var client = CreateAuthorizedClient();
        var response = await client.PatchAsync($"/api/invoices/{id}/issue", null, cancellationToken);
        if (!response.IsSuccessStatusCode)
            TempData["Error"] = "Cannot issue invoice — ensure it has at least one line item.";
        else
            TempData["SuccessMessage"] = "Invoice issued.";
        return RedirectToAction(nameof(InvoiceDetail), new { id });
    }

    [HttpPost("invoices/{id:guid}/void")]
    public async Task<IActionResult> VoidInvoice(Guid id, CancellationToken cancellationToken)
    {
        var client = CreateAuthorizedClient();
        await client.PatchAsync($"/api/invoices/{id}/void", null, cancellationToken);
        TempData["SuccessMessage"] = "Invoice voided.";
        return RedirectToAction(nameof(InvoiceDetail), new { id });
    }

    [HttpPost("invoices/{id:guid}/payments")]
    public async Task<IActionResult> RecordPayment(Guid id, [FromForm] decimal amount, [FromForm] string method,
        [FromForm] Guid recordedByUserId, [FromForm] string? referenceNumber, [FromForm] string? notes, CancellationToken cancellationToken)
    {
        var client = CreateAuthorizedClient();
        var request = new RecordPaymentRequest(id, amount, method, recordedByUserId, referenceNumber, notes);
        var response = await client.PostAsJsonAsync($"/api/invoices/{id}/payments", request, cancellationToken);
        if (!response.IsSuccessStatusCode)
            TempData["Error"] = "Failed to record payment.";
        else
            TempData["SuccessMessage"] = "Payment recorded.";
        return RedirectToAction(nameof(InvoiceDetail), new { id });
    }

    // ── Charge Items ─────────────────────────────────────────────────────────

    [HttpGet("charge-items")]
    public async Task<IActionResult> ChargeItems(CancellationToken cancellationToken)
    {
        var client = CreateAuthorizedClient();
        var items = await client.GetFromJsonAsync<IReadOnlyList<ChargeItemResponse>>("/api/charge-items", cancellationToken) ?? [];
        ViewData["Title"] = "Charge Items";
        ViewData["ActivePage"] = "Billing";
        return View(items);
    }

    [HttpGet("charge-items/create")]
    public IActionResult CreateChargeItem()
    {
        ViewData["Title"] = "New Charge Item";
        ViewData["ActivePage"] = "Billing";
        return View();
    }

    [HttpPost("charge-items/create")]
    public async Task<IActionResult> CreateChargeItem(CreateChargeItemRequest request, CancellationToken cancellationToken)
    {
        var client = CreateAuthorizedClient();
        await client.PostAsJsonAsync("/api/charge-items", request, cancellationToken);
        return RedirectToAction(nameof(ChargeItems));
    }

    [HttpPost("charge-items/{id:guid}/toggle")]
    public async Task<IActionResult> ToggleChargeItem(Guid id, CancellationToken cancellationToken)
    {
        var client = CreateAuthorizedClient();
        await client.PatchAsync($"/api/charge-items/{id}/toggle-active", null, cancellationToken);
        return RedirectToAction(nameof(ChargeItems));
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private HttpClient CreateAuthorizedClient()
    {
        var client = httpClientFactory.CreateClient("HospitalAPI");
        var token = HttpContext.Session.GetString(TokenSessionKey);
        if (!string.IsNullOrEmpty(token))
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }
}
