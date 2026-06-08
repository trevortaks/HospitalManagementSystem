using HospitalMS.Business.Models;

namespace HospitalMS.Business.Services;

public interface ISupplyChainService
{
    // Suppliers
    Task<IReadOnlyList<SupplierResponse>> GetAllSuppliersAsync(bool? activeOnly = null, CancellationToken cancellationToken = default);
    Task<SupplierResponse?> GetSupplierByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<SupplierResponse> CreateSupplierAsync(CreateSupplierRequest request, CancellationToken cancellationToken = default);
    Task<SupplierResponse> ToggleSupplierActiveAsync(Guid id, CancellationToken cancellationToken = default);

    // Purchase Orders
    Task<IReadOnlyList<PurchaseOrderResponse>> GetAllPurchaseOrdersAsync(string? status = null, CancellationToken cancellationToken = default);
    Task<PurchaseOrderResponse?> GetPurchaseOrderByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PurchaseOrderResponse> CreatePurchaseOrderAsync(CreatePurchaseOrderRequest request, CancellationToken cancellationToken = default);
    Task<PurchaseOrderResponse> SubmitPurchaseOrderAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PurchaseOrderResponse> ApprovePurchaseOrderAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PurchaseOrderResponse> CancelPurchaseOrderAsync(Guid id, CancellationToken cancellationToken = default);

    // Goods Receipts
    Task<GoodsReceiptResponse> ReceiveGoodsAsync(ReceiveGoodsRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<GoodsReceiptResponse>> GetReceiptsByPurchaseOrderAsync(Guid purchaseOrderId, CancellationToken cancellationToken = default);
}
