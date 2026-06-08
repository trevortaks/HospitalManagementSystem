using HospitalMS.Business.Models;

namespace HospitalMS.Business.Services;

public interface IInventoryService
{
    // Categories
    Task<IReadOnlyList<InventoryCategoryResponse>> GetAllCategoriesAsync(bool? activeOnly = null, CancellationToken cancellationToken = default);
    Task<InventoryCategoryResponse?> GetCategoryByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<InventoryCategoryResponse> CreateCategoryAsync(CreateInventoryCategoryRequest request, CancellationToken cancellationToken = default);
    Task<InventoryCategoryResponse> ToggleCategoryActiveAsync(Guid id, CancellationToken cancellationToken = default);

    // Items
    Task<IReadOnlyList<InventoryItemResponse>> GetAllItemsAsync(Guid? categoryId = null, bool? lowStockOnly = null, CancellationToken cancellationToken = default);
    Task<InventoryItemResponse?> GetItemByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<InventoryItemResponse> CreateItemAsync(CreateInventoryItemRequest request, CancellationToken cancellationToken = default);
    Task<InventoryItemResponse> UpdateItemAsync(Guid id, UpdateInventoryItemRequest request, CancellationToken cancellationToken = default);
    Task<InventoryItemResponse> ToggleItemActiveAsync(Guid id, CancellationToken cancellationToken = default);

    // Stock operations
    Task<IReadOnlyList<StockTransactionResponse>> GetTransactionsByItemAsync(Guid itemId, CancellationToken cancellationToken = default);
    Task<StockTransactionResponse> StockInAsync(StockInRequest request, CancellationToken cancellationToken = default);
    Task<StockTransactionResponse> StockOutAsync(StockOutRequest request, CancellationToken cancellationToken = default);
    Task<StockTransactionResponse> AdjustStockAsync(AdjustStockRequest request, CancellationToken cancellationToken = default);
}
