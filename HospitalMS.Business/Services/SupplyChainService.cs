using HospitalMS.Business.Models;
using HospitalMS.Data.Persistence;
using HospitalMS.Data.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace HospitalMS.Business.Services;

public sealed class SupplyChainService(HospitalDbContext dbContext) : ISupplyChainService
{
    // ── Suppliers ────────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<SupplierResponse>> GetAllSuppliersAsync(bool? activeOnly = null, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Suppliers.AsNoTracking().AsQueryable();
        if (activeOnly == true) query = query.Where(s => s.IsActive);
        var suppliers = await query.OrderBy(s => s.Name).ToListAsync(cancellationToken);
        return suppliers.Select(ToSupplierResponse).ToArray();
    }

    public async Task<SupplierResponse?> GetSupplierByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var supplier = await dbContext.Suppliers.AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
        return supplier is null ? null : ToSupplierResponse(supplier);
    }

    public async Task<SupplierResponse> CreateSupplierAsync(CreateSupplierRequest request, CancellationToken cancellationToken = default)
    {
        var supplier = new Supplier
        {
            Name = request.Name,
            ContactName = request.ContactName,
            ContactPhone = request.ContactPhone,
            ContactEmail = request.ContactEmail,
            Address = request.Address
        };
        dbContext.Suppliers.Add(supplier);
        await dbContext.SaveChangesAsync(cancellationToken);
        return ToSupplierResponse(supplier);
    }

    public async Task<SupplierResponse> ToggleSupplierActiveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var supplier = await dbContext.Suppliers.FindAsync([id], cancellationToken)
            ?? throw new KeyNotFoundException($"Supplier '{id}' not found.");
        supplier.IsActive = !supplier.IsActive;
        await dbContext.SaveChangesAsync(cancellationToken);
        return ToSupplierResponse(supplier);
    }

    // ── Purchase Orders ───────────────────────────────────────────────────────

    public async Task<IReadOnlyList<PurchaseOrderResponse>> GetAllPurchaseOrdersAsync(string? status = null, CancellationToken cancellationToken = default)
    {
        var query = dbContext.PurchaseOrders.AsNoTracking()
            .Include(p => p.Supplier)
            .Include(p => p.OrderedByUser)
            .Include(p => p.Lines).ThenInclude(l => l.InventoryItem)
            .AsQueryable();
        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(p => p.Status == status);
        var orders = await query.OrderByDescending(p => p.CreatedAtUtc).ToListAsync(cancellationToken);
        return orders.Select(ToPurchaseOrderResponse).ToArray();
    }

    public async Task<PurchaseOrderResponse?> GetPurchaseOrderByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var po = await LoadPurchaseOrderAsync(id, cancellationToken);
        return po is null ? null : ToPurchaseOrderResponse(po);
    }

    public async Task<PurchaseOrderResponse> CreatePurchaseOrderAsync(CreatePurchaseOrderRequest request, CancellationToken cancellationToken = default)
    {
        if (!request.Lines.Any())
            throw new InvalidOperationException("A purchase order must have at least one line.");

        var orderNumber = $"PO-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}"[..18].ToUpperInvariant();
        var po = new PurchaseOrder
        {
            SupplierId = request.SupplierId,
            OrderedByUserId = request.OrderedByUserId,
            OrderNumber = orderNumber,
            Notes = request.Notes,
            ExpectedDeliveryDate = request.ExpectedDeliveryDate
        };
        dbContext.PurchaseOrders.Add(po);

        foreach (var lineReq in request.Lines)
        {
            var item = await dbContext.InventoryItems.FindAsync([lineReq.InventoryItemId], cancellationToken)
                ?? throw new KeyNotFoundException($"Inventory item '{lineReq.InventoryItemId}' not found.");

            dbContext.PurchaseOrderLines.Add(new PurchaseOrderLine
            {
                PurchaseOrderId = po.Id,
                InventoryItemId = lineReq.InventoryItemId,
                Description = item.Name,
                QuantityOrdered = lineReq.QuantityOrdered,
                UnitCost = lineReq.UnitCost,
                TotalCost = lineReq.UnitCost * lineReq.QuantityOrdered
            });
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return ToPurchaseOrderResponse(await LoadPurchaseOrderAsync(po.Id, cancellationToken)!);
    }

    public async Task<PurchaseOrderResponse> SubmitPurchaseOrderAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var po = await dbContext.PurchaseOrders.FindAsync([id], cancellationToken)
            ?? throw new KeyNotFoundException($"Purchase order '{id}' not found.");
        if (po.Status != PurchaseOrderStatus.Draft)
            throw new InvalidOperationException("Only draft purchase orders can be submitted.");
        po.Status = PurchaseOrderStatus.Submitted;
        po.UpdatedAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        return ToPurchaseOrderResponse(await LoadPurchaseOrderAsync(id, cancellationToken)!);
    }

    public async Task<PurchaseOrderResponse> ApprovePurchaseOrderAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var po = await dbContext.PurchaseOrders.FindAsync([id], cancellationToken)
            ?? throw new KeyNotFoundException($"Purchase order '{id}' not found.");
        if (po.Status != PurchaseOrderStatus.Submitted)
            throw new InvalidOperationException("Only submitted purchase orders can be approved.");
        po.Status = PurchaseOrderStatus.Approved;
        po.UpdatedAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        return ToPurchaseOrderResponse(await LoadPurchaseOrderAsync(id, cancellationToken)!);
    }

    public async Task<PurchaseOrderResponse> CancelPurchaseOrderAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var po = await dbContext.PurchaseOrders.FindAsync([id], cancellationToken)
            ?? throw new KeyNotFoundException($"Purchase order '{id}' not found.");
        if (po.Status == PurchaseOrderStatus.Received)
            throw new InvalidOperationException("Cannot cancel a fully received purchase order.");
        if (po.Status == PurchaseOrderStatus.Cancelled)
            throw new InvalidOperationException("Purchase order is already cancelled.");
        po.Status = PurchaseOrderStatus.Cancelled;
        po.UpdatedAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        return ToPurchaseOrderResponse(await LoadPurchaseOrderAsync(id, cancellationToken)!);
    }

    // ── Goods Receipts ────────────────────────────────────────────────────────

    public async Task<GoodsReceiptResponse> ReceiveGoodsAsync(ReceiveGoodsRequest request, CancellationToken cancellationToken = default)
    {
        var po = await dbContext.PurchaseOrders
            .Include(p => p.Lines).ThenInclude(l => l.InventoryItem)
            .FirstOrDefaultAsync(p => p.Id == request.PurchaseOrderId, cancellationToken)
            ?? throw new KeyNotFoundException($"Purchase order '{request.PurchaseOrderId}' not found.");

        if (po.Status is PurchaseOrderStatus.Draft or PurchaseOrderStatus.Submitted
                        or PurchaseOrderStatus.Cancelled)
            throw new InvalidOperationException($"Cannot receive goods for a {po.Status.ToLowerInvariant()} purchase order. It must be Approved or PartiallyReceived.");

        if (!request.Lines.Any())
            throw new InvalidOperationException("At least one line must be included in a goods receipt.");

        var receipt = new GoodsReceipt
        {
            PurchaseOrderId = request.PurchaseOrderId,
            ReceivedByUserId = request.ReceivedByUserId,
            Notes = request.Notes
        };
        dbContext.GoodsReceipts.Add(receipt);

        foreach (var lineReq in request.Lines)
        {
            if (lineReq.QuantityReceived <= 0) continue;

            var poLine = po.Lines.FirstOrDefault(l => l.Id == lineReq.PurchaseOrderLineId)
                ?? throw new KeyNotFoundException($"Purchase order line '{lineReq.PurchaseOrderLineId}' not found.");

            var remaining = poLine.QuantityOrdered - poLine.QuantityReceived;
            var toReceive = Math.Min(lineReq.QuantityReceived, remaining);
            if (toReceive <= 0) continue;

            poLine.QuantityReceived += toReceive;

            dbContext.GoodsReceiptLines.Add(new GoodsReceiptLine
            {
                GoodsReceiptId = receipt.Id,
                PurchaseOrderLineId = poLine.Id,
                QuantityReceived = toReceive
            });

            // Stock in the received quantity
            var item = await dbContext.InventoryItems.FindAsync([poLine.InventoryItemId], cancellationToken)
                ?? throw new KeyNotFoundException($"Inventory item '{poLine.InventoryItemId}' not found.");
            item.CurrentStock += toReceive;

            dbContext.StockTransactions.Add(new StockTransaction
            {
                ItemId = poLine.InventoryItemId,
                Type = StockTransactionType.In,
                Quantity = toReceive,
                StockAfter = item.CurrentStock,
                ReferenceType = "PurchaseOrder",
                ReferenceId = po.Id,
                Notes = $"Received via {po.OrderNumber}",
                PerformedByUserId = request.ReceivedByUserId
            });
        }

        // Update PO status
        var allFullyReceived = po.Lines.All(l => l.QuantityReceived >= l.QuantityOrdered);
        po.Status = allFullyReceived ? PurchaseOrderStatus.Received : PurchaseOrderStatus.PartiallyReceived;
        po.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        return ToGoodsReceiptResponse(await LoadReceiptAsync(receipt.Id, cancellationToken)!);
    }

    public async Task<IReadOnlyList<GoodsReceiptResponse>> GetReceiptsByPurchaseOrderAsync(Guid purchaseOrderId, CancellationToken cancellationToken = default)
    {
        var receipts = await dbContext.GoodsReceipts.AsNoTracking()
            .Include(r => r.PurchaseOrder)
            .Include(r => r.ReceivedByUser)
            .Include(r => r.Lines).ThenInclude(l => l.PurchaseOrderLine).ThenInclude(l => l.InventoryItem)
            .Where(r => r.PurchaseOrderId == purchaseOrderId)
            .OrderByDescending(r => r.ReceivedAtUtc)
            .ToListAsync(cancellationToken);
        return receipts.Select(ToGoodsReceiptResponse).ToArray();
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private async Task<PurchaseOrder?> LoadPurchaseOrderAsync(Guid id, CancellationToken ct) =>
        await dbContext.PurchaseOrders.AsNoTracking()
            .Include(p => p.Supplier)
            .Include(p => p.OrderedByUser)
            .Include(p => p.Lines).ThenInclude(l => l.InventoryItem)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

    private async Task<GoodsReceipt?> LoadReceiptAsync(Guid id, CancellationToken ct) =>
        await dbContext.GoodsReceipts.AsNoTracking()
            .Include(r => r.PurchaseOrder)
            .Include(r => r.ReceivedByUser)
            .Include(r => r.Lines).ThenInclude(l => l.PurchaseOrderLine).ThenInclude(l => l.InventoryItem)
            .FirstOrDefaultAsync(r => r.Id == id, ct);

    private static SupplierResponse ToSupplierResponse(Supplier s) =>
        new(s.Id, s.Name, s.ContactName, s.ContactPhone, s.ContactEmail, s.Address, s.IsActive, s.CreatedAtUtc);

    private static PurchaseOrderResponse ToPurchaseOrderResponse(PurchaseOrder p) =>
        new(p.Id, p.OrderNumber, p.SupplierId, p.Supplier.Name, p.OrderedByUser.Username,
            p.Status, p.Notes, p.ExpectedDeliveryDate, p.OrderedAtUtc, p.CreatedAtUtc,
            p.Lines.Sum(l => l.TotalCost),
            p.Lines.Select(l => new PurchaseOrderLineResponse(
                l.Id, l.InventoryItemId, l.InventoryItem.Code, l.InventoryItem.Name,
                l.Description, l.QuantityOrdered, l.QuantityReceived, l.UnitCost, l.TotalCost)).ToArray());

    private static GoodsReceiptResponse ToGoodsReceiptResponse(GoodsReceipt r) =>
        new(r.Id, r.PurchaseOrderId, r.PurchaseOrder.OrderNumber, r.ReceivedByUser.Username,
            r.Notes, r.ReceivedAtUtc,
            r.Lines.Select(l => new GoodsReceiptLineResponse(
                l.PurchaseOrderLineId, l.PurchaseOrderLine.InventoryItem.Code,
                l.PurchaseOrderLine.InventoryItem.Name, l.QuantityReceived)).ToArray());
}
