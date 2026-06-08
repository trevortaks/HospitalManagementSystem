using HospitalMS.Business.Models;
using HospitalMS.Data.Persistence;
using HospitalMS.Data.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace HospitalMS.Business.Services;

public sealed class InventoryService(HospitalDbContext dbContext) : IInventoryService
{
    // ── Categories ───────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<InventoryCategoryResponse>> GetAllCategoriesAsync(bool? activeOnly = null, CancellationToken cancellationToken = default)
    {
        var query = dbContext.InventoryCategories.AsNoTracking()
            .Include(c => c.Items)
            .AsQueryable();
        if (activeOnly == true) query = query.Where(c => c.IsActive);
        var cats = await query.OrderBy(c => c.Name).ToListAsync(cancellationToken);
        return cats.Select(ToCategoryResponse).ToArray();
    }

    public async Task<InventoryCategoryResponse?> GetCategoryByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var cat = await dbContext.InventoryCategories.AsNoTracking()
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        return cat is null ? null : ToCategoryResponse(cat);
    }

    public async Task<InventoryCategoryResponse> CreateCategoryAsync(CreateInventoryCategoryRequest request, CancellationToken cancellationToken = default)
    {
        var cat = new InventoryCategory { Name = request.Name, Description = request.Description };
        dbContext.InventoryCategories.Add(cat);
        await dbContext.SaveChangesAsync(cancellationToken);
        return ToCategoryResponse(await LoadCategoryAsync(cat.Id, cancellationToken)!);
    }

    public async Task<InventoryCategoryResponse> ToggleCategoryActiveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var cat = await dbContext.InventoryCategories.FindAsync([id], cancellationToken)
            ?? throw new KeyNotFoundException($"Category '{id}' not found.");
        cat.IsActive = !cat.IsActive;
        await dbContext.SaveChangesAsync(cancellationToken);
        return ToCategoryResponse(await LoadCategoryAsync(id, cancellationToken)!);
    }

    // ── Items ────────────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<InventoryItemResponse>> GetAllItemsAsync(Guid? categoryId = null, bool? lowStockOnly = null, CancellationToken cancellationToken = default)
    {
        var query = dbContext.InventoryItems.AsNoTracking()
            .Include(i => i.Category)
            .AsQueryable();
        if (categoryId.HasValue) query = query.Where(i => i.CategoryId == categoryId.Value);
        var items = await query.OrderBy(i => i.Name).ToListAsync(cancellationToken);
        var responses = items.Select(ToItemResponse).ToArray();
        if (lowStockOnly == true) responses = [.. responses.Where(r => r.IsLowStock)];
        return responses;
    }

    public async Task<InventoryItemResponse?> GetItemByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await dbContext.InventoryItems.AsNoTracking()
            .Include(i => i.Category)
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
        return item is null ? null : ToItemResponse(item);
    }

    public async Task<InventoryItemResponse> CreateItemAsync(CreateInventoryItemRequest request, CancellationToken cancellationToken = default)
    {
        _ = await dbContext.InventoryCategories.FindAsync([request.CategoryId], cancellationToken)
            ?? throw new KeyNotFoundException($"Category '{request.CategoryId}' not found.");

        var item = new InventoryItem
        {
            CategoryId = request.CategoryId,
            Code = request.Code,
            Name = request.Name,
            Description = request.Description,
            Unit = request.Unit,
            ReorderLevel = request.ReorderLevel,
            UnitCost = request.UnitCost
        };
        dbContext.InventoryItems.Add(item);
        await dbContext.SaveChangesAsync(cancellationToken);
        return ToItemResponse(await LoadItemAsync(item.Id, cancellationToken)!);
    }

    public async Task<InventoryItemResponse> UpdateItemAsync(Guid id, UpdateInventoryItemRequest request, CancellationToken cancellationToken = default)
    {
        var item = await dbContext.InventoryItems.FindAsync([id], cancellationToken)
            ?? throw new KeyNotFoundException($"Item '{id}' not found.");
        item.Name = request.Name;
        item.Description = request.Description;
        item.Unit = request.Unit;
        item.ReorderLevel = request.ReorderLevel;
        item.UnitCost = request.UnitCost;
        await dbContext.SaveChangesAsync(cancellationToken);
        return ToItemResponse(await LoadItemAsync(id, cancellationToken)!);
    }

    public async Task<InventoryItemResponse> ToggleItemActiveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await dbContext.InventoryItems.FindAsync([id], cancellationToken)
            ?? throw new KeyNotFoundException($"Item '{id}' not found.");
        item.IsActive = !item.IsActive;
        await dbContext.SaveChangesAsync(cancellationToken);
        return ToItemResponse(await LoadItemAsync(id, cancellationToken)!);
    }

    // ── Stock Operations ─────────────────────────────────────────────────────

    public async Task<IReadOnlyList<StockTransactionResponse>> GetTransactionsByItemAsync(Guid itemId, CancellationToken cancellationToken = default)
    {
        var txs = await dbContext.StockTransactions.AsNoTracking()
            .Include(t => t.Item)
            .Include(t => t.PerformedByUser)
            .Where(t => t.ItemId == itemId)
            .OrderByDescending(t => t.TransactedAtUtc)
            .ToListAsync(cancellationToken);
        return txs.Select(ToTransactionResponse).ToArray();
    }

    public async Task<StockTransactionResponse> StockInAsync(StockInRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Quantity <= 0) throw new ArgumentException("Quantity must be greater than zero.");
        var item = await dbContext.InventoryItems.FindAsync([request.ItemId], cancellationToken)
            ?? throw new KeyNotFoundException($"Item '{request.ItemId}' not found.");

        item.CurrentStock += request.Quantity;
        var tx = new StockTransaction
        {
            ItemId = request.ItemId,
            Type = StockTransactionType.In,
            Quantity = request.Quantity,
            StockAfter = item.CurrentStock,
            ReferenceType = request.ReferenceType,
            ReferenceId = request.ReferenceId,
            Notes = request.Notes,
            PerformedByUserId = request.PerformedByUserId
        };
        dbContext.StockTransactions.Add(tx);
        await dbContext.SaveChangesAsync(cancellationToken);
        return ToTransactionResponse(await LoadTransactionAsync(tx.Id, cancellationToken)!);
    }

    public async Task<StockTransactionResponse> StockOutAsync(StockOutRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Quantity <= 0) throw new ArgumentException("Quantity must be greater than zero.");
        var item = await dbContext.InventoryItems.FindAsync([request.ItemId], cancellationToken)
            ?? throw new KeyNotFoundException($"Item '{request.ItemId}' not found.");

        if (item.CurrentStock < request.Quantity)
            throw new InvalidOperationException($"Insufficient stock. Available: {item.CurrentStock}, requested: {request.Quantity}.");

        item.CurrentStock -= request.Quantity;
        var tx = new StockTransaction
        {
            ItemId = request.ItemId,
            Type = StockTransactionType.Out,
            Quantity = request.Quantity,
            StockAfter = item.CurrentStock,
            ReferenceType = request.ReferenceType,
            ReferenceId = request.ReferenceId,
            Notes = request.Notes,
            PerformedByUserId = request.PerformedByUserId
        };
        dbContext.StockTransactions.Add(tx);
        await dbContext.SaveChangesAsync(cancellationToken);
        return ToTransactionResponse(await LoadTransactionAsync(tx.Id, cancellationToken)!);
    }

    public async Task<StockTransactionResponse> AdjustStockAsync(AdjustStockRequest request, CancellationToken cancellationToken = default)
    {
        if (request.NewQuantity < 0) throw new ArgumentException("Stock quantity cannot be negative.");
        var item = await dbContext.InventoryItems.FindAsync([request.ItemId], cancellationToken)
            ?? throw new KeyNotFoundException($"Item '{request.ItemId}' not found.");

        var delta = request.NewQuantity - item.CurrentStock;
        item.CurrentStock = request.NewQuantity;
        var tx = new StockTransaction
        {
            ItemId = request.ItemId,
            Type = StockTransactionType.Adjustment,
            Quantity = delta,
            StockAfter = item.CurrentStock,
            Notes = request.Notes,
            PerformedByUserId = request.PerformedByUserId
        };
        dbContext.StockTransactions.Add(tx);
        await dbContext.SaveChangesAsync(cancellationToken);
        return ToTransactionResponse(await LoadTransactionAsync(tx.Id, cancellationToken)!);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private async Task<InventoryCategory?> LoadCategoryAsync(Guid id, CancellationToken ct) =>
        await dbContext.InventoryCategories.AsNoTracking()
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.Id == id, ct);

    private async Task<InventoryItem?> LoadItemAsync(Guid id, CancellationToken ct) =>
        await dbContext.InventoryItems.AsNoTracking()
            .Include(i => i.Category)
            .FirstOrDefaultAsync(i => i.Id == id, ct);

    private async Task<StockTransaction?> LoadTransactionAsync(Guid id, CancellationToken ct) =>
        await dbContext.StockTransactions.AsNoTracking()
            .Include(t => t.Item)
            .Include(t => t.PerformedByUser)
            .FirstOrDefaultAsync(t => t.Id == id, ct);

    private static InventoryCategoryResponse ToCategoryResponse(InventoryCategory c) =>
        new(c.Id, c.Name, c.Description, c.IsActive, c.CreatedAtUtc, c.Items.Count);

    private static InventoryItemResponse ToItemResponse(InventoryItem i) =>
        new(i.Id, i.CategoryId, i.Category.Name, i.Code, i.Name, i.Description,
            i.Unit, i.ReorderLevel, i.CurrentStock, i.UnitCost, i.IsActive, i.CreatedAtUtc,
            i.CurrentStock <= i.ReorderLevel);

    private static StockTransactionResponse ToTransactionResponse(StockTransaction t) =>
        new(t.Id, t.ItemId, t.Item.Code, t.Item.Name, t.Type, t.Quantity, t.StockAfter,
            t.ReferenceType, t.ReferenceId, t.Notes, t.PerformedByUser.Username, t.TransactedAtUtc);
}
