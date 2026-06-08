namespace HospitalMS.Business.Models;

public record InventoryCategoryResponse(
    Guid Id,
    string Name,
    string? Description,
    bool IsActive,
    DateTime CreatedAtUtc,
    int ItemCount);

public record CreateInventoryCategoryRequest(
    string Name,
    string? Description = null);

public record InventoryItemResponse(
    Guid Id,
    Guid CategoryId,
    string CategoryName,
    string Code,
    string Name,
    string? Description,
    string Unit,
    int ReorderLevel,
    int CurrentStock,
    decimal UnitCost,
    bool IsActive,
    DateTime CreatedAtUtc,
    bool IsLowStock);

public record CreateInventoryItemRequest(
    Guid CategoryId,
    string Code,
    string Name,
    string Unit,
    int ReorderLevel,
    decimal UnitCost,
    string? Description = null);

public record UpdateInventoryItemRequest(
    string Name,
    string Unit,
    int ReorderLevel,
    decimal UnitCost,
    string? Description = null);

public record StockTransactionResponse(
    Guid Id,
    Guid ItemId,
    string ItemCode,
    string ItemName,
    string Type,
    int Quantity,
    int StockAfter,
    string? ReferenceType,
    Guid? ReferenceId,
    string? Notes,
    string PerformedByUsername,
    DateTime TransactedAtUtc);

public record StockInRequest(
    Guid ItemId,
    int Quantity,
    Guid PerformedByUserId,
    string? Notes = null,
    string? ReferenceType = null,
    Guid? ReferenceId = null);

public record StockOutRequest(
    Guid ItemId,
    int Quantity,
    Guid PerformedByUserId,
    string? Notes = null,
    string? ReferenceType = null,
    Guid? ReferenceId = null);

public record AdjustStockRequest(
    Guid ItemId,
    int NewQuantity,
    Guid PerformedByUserId,
    string? Notes = null);
