namespace HospitalMS.Business.Models;

public record SupplierResponse(
    Guid Id,
    string Name,
    string? ContactName,
    string? ContactPhone,
    string? ContactEmail,
    string? Address,
    bool IsActive,
    DateTime CreatedAtUtc);

public record CreateSupplierRequest(
    string Name,
    string? ContactName = null,
    string? ContactPhone = null,
    string? ContactEmail = null,
    string? Address = null);

public record PurchaseOrderLineResponse(
    Guid Id,
    Guid InventoryItemId,
    string ItemCode,
    string ItemName,
    string Description,
    int QuantityOrdered,
    int QuantityReceived,
    decimal UnitCost,
    decimal TotalCost);

public record PurchaseOrderResponse(
    Guid Id,
    string OrderNumber,
    Guid SupplierId,
    string SupplierName,
    string OrderedByUsername,
    string Status,
    string? Notes,
    DateTime? ExpectedDeliveryDate,
    DateTime OrderedAtUtc,
    DateTime CreatedAtUtc,
    decimal TotalOrderValue,
    IReadOnlyList<PurchaseOrderLineResponse> Lines);

public record CreatePurchaseOrderLineRequest(
    Guid InventoryItemId,
    int QuantityOrdered,
    decimal UnitCost);

public record CreatePurchaseOrderRequest(
    Guid SupplierId,
    Guid OrderedByUserId,
    IReadOnlyList<CreatePurchaseOrderLineRequest> Lines,
    string? Notes = null,
    DateTime? ExpectedDeliveryDate = null);

public record ReceivePOLineRequest(
    Guid PurchaseOrderLineId,
    int QuantityReceived);

public record ReceiveGoodsRequest(
    Guid PurchaseOrderId,
    Guid ReceivedByUserId,
    IReadOnlyList<ReceivePOLineRequest> Lines,
    string? Notes = null);

public record GoodsReceiptLineResponse(
    Guid PurchaseOrderLineId,
    string ItemCode,
    string ItemName,
    int QuantityReceived);

public record GoodsReceiptResponse(
    Guid Id,
    Guid PurchaseOrderId,
    string OrderNumber,
    string ReceivedByUsername,
    string? Notes,
    DateTime ReceivedAtUtc,
    IReadOnlyList<GoodsReceiptLineResponse> Lines);
