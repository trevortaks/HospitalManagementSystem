using HospitalMS.Business.Models;
using HospitalMS.Data.Persistence;
using HospitalMS.Data.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace HospitalMS.Business.Services;

public sealed class BillingService(HospitalDbContext dbContext) : IBillingService
{
    // ── Charge Items ─────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<ChargeItemResponse>> GetAllChargeItemsAsync(bool? activeOnly = null, CancellationToken cancellationToken = default)
    {
        var query = dbContext.ChargeItems.AsNoTracking().AsQueryable();
        if (activeOnly == true) query = query.Where(c => c.IsActive);
        var items = await query.OrderBy(c => c.Code).ToListAsync(cancellationToken);
        return items.Select(ToChargeItemResponse).ToArray();
    }

    public async Task<ChargeItemResponse?> GetChargeItemByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await dbContext.ChargeItems.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        return item is null ? null : ToChargeItemResponse(item);
    }

    public async Task<ChargeItemResponse> CreateChargeItemAsync(CreateChargeItemRequest request, CancellationToken cancellationToken = default)
    {
        var item = new ChargeItem
        {
            Code = request.Code,
            Description = request.Description,
            Category = request.Category,
            UnitPrice = request.UnitPrice,
            IsActive = request.IsActive
        };
        dbContext.ChargeItems.Add(item);
        await dbContext.SaveChangesAsync(cancellationToken);
        return ToChargeItemResponse(item);
    }

    public async Task<ChargeItemResponse> UpdateChargeItemAsync(Guid id, UpdateChargeItemRequest request, CancellationToken cancellationToken = default)
    {
        var item = await dbContext.ChargeItems.FindAsync([id], cancellationToken)
            ?? throw new KeyNotFoundException($"Charge item '{id}' not found.");
        item.Description = request.Description;
        item.Category = request.Category;
        item.UnitPrice = request.UnitPrice;
        await dbContext.SaveChangesAsync(cancellationToken);
        return ToChargeItemResponse(item);
    }

    public async Task<ChargeItemResponse> ToggleChargeItemActiveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await dbContext.ChargeItems.FindAsync([id], cancellationToken)
            ?? throw new KeyNotFoundException($"Charge item '{id}' not found.");
        item.IsActive = !item.IsActive;
        await dbContext.SaveChangesAsync(cancellationToken);
        return ToChargeItemResponse(item);
    }

    // ── Invoices ─────────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<InvoiceResponse>> GetAllInvoicesAsync(Guid? patientId = null, string? status = null, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Invoices
            .AsNoTracking()
            .Include(i => i.Patient)
            .Include(i => i.CreatedByUser)
            .Include(i => i.LineItems).ThenInclude(li => li.ChargeItem)
            .Include(i => i.Payments).ThenInclude(p => p.RecordedByUser)
            .AsQueryable();

        if (patientId.HasValue) query = query.Where(i => i.PatientId == patientId.Value);
        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(i => i.Status == status);

        var invoices = await query.OrderByDescending(i => i.CreatedAtUtc).ToListAsync(cancellationToken);
        return invoices.Select(ToInvoiceResponse).ToArray();
    }

    public async Task<InvoiceResponse?> GetInvoiceByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var invoice = await LoadInvoiceAsync(id, cancellationToken);
        return invoice is null ? null : ToInvoiceResponse(invoice);
    }

    public async Task<InvoiceResponse> CreateInvoiceAsync(CreateInvoiceRequest request, CancellationToken cancellationToken = default)
    {
        var invoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}"[..22].ToUpperInvariant();
        var invoice = new Invoice
        {
            InvoiceNumber = invoiceNumber,
            PatientId = request.PatientId,
            EncounterId = request.EncounterId,
            CreatedByUserId = request.CreatedByUserId,
            DueDate = request.DueDate,
            Notes = request.Notes
        };
        dbContext.Invoices.Add(invoice);
        await dbContext.SaveChangesAsync(cancellationToken);
        return ToInvoiceResponse(await LoadInvoiceAsync(invoice.Id, cancellationToken)!);
    }

    public async Task<InvoiceResponse> UpdateInvoiceAsync(Guid id, UpdateInvoiceRequest request, CancellationToken cancellationToken = default)
    {
        var invoice = await dbContext.Invoices.FindAsync([id], cancellationToken)
            ?? throw new KeyNotFoundException($"Invoice '{id}' not found.");

        if (invoice.Status != InvoiceStatus.Draft)
            throw new InvalidOperationException("Only draft invoices can be edited.");

        invoice.DiscountAmount = request.DiscountAmount;
        invoice.TaxAmount = request.TaxAmount;
        invoice.DueDate = request.DueDate;
        invoice.Notes = request.Notes;
        invoice.UpdatedAtUtc = DateTime.UtcNow;
        RecalculateTotals(invoice);
        await dbContext.SaveChangesAsync(cancellationToken);
        return ToInvoiceResponse(await LoadInvoiceAsync(id, cancellationToken)!);
    }

    public async Task<InvoiceResponse> AddLineItemAsync(Guid invoiceId, AddLineItemRequest request, CancellationToken cancellationToken = default)
    {
        var invoice = await dbContext.Invoices
            .Include(i => i.LineItems)
            .FirstOrDefaultAsync(i => i.Id == invoiceId, cancellationToken)
            ?? throw new KeyNotFoundException($"Invoice '{invoiceId}' not found.");

        if (invoice.Status != InvoiceStatus.Draft)
            throw new InvalidOperationException("Line items can only be added to draft invoices.");

        var chargeItem = await dbContext.ChargeItems.FindAsync([request.ChargeItemId], cancellationToken)
            ?? throw new KeyNotFoundException($"Charge item '{request.ChargeItemId}' not found.");

        var lineItem = new InvoiceLineItem
        {
            InvoiceId = invoiceId,
            ChargeItemId = chargeItem.Id,
            Description = chargeItem.Description,
            Quantity = request.Quantity,
            UnitPrice = chargeItem.UnitPrice,
            TotalPrice = chargeItem.UnitPrice * request.Quantity
        };
        dbContext.InvoiceLineItems.Add(lineItem);
        RecalculateTotals(invoice);
        invoice.UpdatedAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        return ToInvoiceResponse(await LoadInvoiceAsync(invoiceId, cancellationToken)!);
    }

    public async Task<InvoiceResponse> RemoveLineItemAsync(Guid invoiceId, Guid lineItemId, CancellationToken cancellationToken = default)
    {
        var invoice = await dbContext.Invoices
            .Include(i => i.LineItems)
            .FirstOrDefaultAsync(i => i.Id == invoiceId, cancellationToken)
            ?? throw new KeyNotFoundException($"Invoice '{invoiceId}' not found.");

        if (invoice.Status != InvoiceStatus.Draft)
            throw new InvalidOperationException("Line items can only be removed from draft invoices.");

        var lineItem = invoice.LineItems.FirstOrDefault(li => li.Id == lineItemId)
            ?? throw new KeyNotFoundException($"Line item '{lineItemId}' not found.");

        dbContext.InvoiceLineItems.Remove(lineItem);
        invoice.LineItems.Remove(lineItem);
        RecalculateTotals(invoice);
        invoice.UpdatedAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        return ToInvoiceResponse(await LoadInvoiceAsync(invoiceId, cancellationToken)!);
    }

    public async Task<InvoiceResponse> IssueInvoiceAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var invoice = await dbContext.Invoices
            .Include(i => i.LineItems)
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException($"Invoice '{id}' not found.");

        if (invoice.Status != InvoiceStatus.Draft)
            throw new InvalidOperationException("Only draft invoices can be issued.");

        if (!invoice.LineItems.Any())
            throw new InvalidOperationException("Cannot issue an invoice with no line items.");

        invoice.Status = InvoiceStatus.Issued;
        invoice.UpdatedAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        return ToInvoiceResponse(await LoadInvoiceAsync(id, cancellationToken)!);
    }

    public async Task<InvoiceResponse> VoidInvoiceAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var invoice = await dbContext.Invoices.FindAsync([id], cancellationToken)
            ?? throw new KeyNotFoundException($"Invoice '{id}' not found.");

        if (invoice.Status == InvoiceStatus.Paid)
            throw new InvalidOperationException("Cannot void a fully paid invoice.");

        if (invoice.Status == InvoiceStatus.Voided)
            throw new InvalidOperationException("Invoice is already voided.");

        invoice.Status = InvoiceStatus.Voided;
        invoice.UpdatedAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        return ToInvoiceResponse(await LoadInvoiceAsync(id, cancellationToken)!);
    }

    // ── Payments ─────────────────────────────────────────────────────────────

    public async Task<InvoiceResponse> RecordPaymentAsync(RecordPaymentRequest request, CancellationToken cancellationToken = default)
    {
        var invoice = await dbContext.Invoices.FindAsync([request.InvoiceId], cancellationToken)
            ?? throw new KeyNotFoundException($"Invoice '{request.InvoiceId}' not found.");

        if (invoice.Status is InvoiceStatus.Voided or InvoiceStatus.Draft)
            throw new InvalidOperationException($"Cannot record payment on a {invoice.Status.ToLowerInvariant()} invoice.");

        if (request.Amount <= 0)
            throw new ArgumentException("Payment amount must be greater than zero.");

        var payment = new Payment
        {
            InvoiceId = request.InvoiceId,
            RecordedByUserId = request.RecordedByUserId,
            Amount = request.Amount,
            Method = request.Method,
            ReferenceNumber = request.ReferenceNumber,
            Notes = request.Notes
        };
        dbContext.Payments.Add(payment);

        invoice.PaidAmount += request.Amount;
        invoice.Status = invoice.PaidAmount >= invoice.TotalAmount
            ? InvoiceStatus.Paid
            : InvoiceStatus.PartiallyPaid;
        invoice.UpdatedAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        return ToInvoiceResponse(await LoadInvoiceAsync(request.InvoiceId, cancellationToken)!);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static void RecalculateTotals(Invoice invoice)
    {
        invoice.SubtotalAmount = invoice.LineItems.Sum(li => li.TotalPrice);
        invoice.TotalAmount = invoice.SubtotalAmount - invoice.DiscountAmount + invoice.TaxAmount;
    }

    private async Task<Invoice?> LoadInvoiceAsync(Guid id, CancellationToken cancellationToken) =>
        await dbContext.Invoices
            .AsNoTracking()
            .Include(i => i.Patient)
            .Include(i => i.CreatedByUser)
            .Include(i => i.LineItems).ThenInclude(li => li.ChargeItem)
            .Include(i => i.Payments).ThenInclude(p => p.RecordedByUser)
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);

    private static ChargeItemResponse ToChargeItemResponse(ChargeItem c) =>
        new(c.Id, c.Code, c.Description, c.Category, c.UnitPrice, c.IsActive, c.CreatedAtUtc);

    private static InvoiceResponse ToInvoiceResponse(Invoice i) => new(
        i.Id, i.InvoiceNumber,
        i.PatientId, $"{i.Patient.FirstName} {i.Patient.LastName}", i.Patient.MedicalRecordNumber,
        i.EncounterId, i.Status,
        i.SubtotalAmount, i.DiscountAmount, i.TaxAmount, i.TotalAmount, i.PaidAmount,
        i.TotalAmount - i.PaidAmount,
        i.DueDate, i.Notes,
        i.CreatedByUser.Username,
        i.CreatedAtUtc, i.UpdatedAtUtc,
        i.LineItems.Select(li => new InvoiceLineItemResponse(
            li.Id, li.ChargeItemId, li.ChargeItem.Code,
            li.Description, li.Quantity, li.UnitPrice, li.TotalPrice)).ToArray(),
        i.Payments.Select(p => new PaymentResponse(
            p.Id, p.InvoiceId, p.Amount, p.Method,
            p.ReferenceNumber, p.Notes,
            p.RecordedByUser.Username, p.PaidAtUtc)).ToArray());
}
