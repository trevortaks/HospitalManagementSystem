using HospitalMS.Business.Models;
using HospitalMS.Business.Services;

namespace HospitalMS.Tests.UnitTests;

public sealed class InventoryServiceTests : IntegrationTestBase
{
    private InventoryService _service = null!;
    private Guid _categoryId;
    private Guid _itemId;
    private Guid _pharmacistId;

    protected override async Task SeedAsync(HospitalMS.Data.Persistence.HospitalDbContext context)
    {
        var category   = TestFixtures.CreateInventoryCategory();
        var item       = TestFixtures.CreateInventoryItem(categoryId: category.Id, currentStock: 200);
        var pharmacist = TestFixtures.CreateUser(role: "Pharmacist");

        await context.InventoryCategories.AddAsync(category);
        await context.InventoryItems.AddAsync(item);
        await context.Users.AddAsync(pharmacist);
        await context.SaveChangesAsync();

        _categoryId  = category.Id;
        _itemId      = item.Id;
        _pharmacistId = pharmacist.Id;
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _service = new InventoryService(Context);
    }

    [Fact]
    public async Task CreateCategoryAsync_ReturnsSavedCategory()
    {
        var request = new CreateInventoryCategoryRequest("Surgical Supplies", "Consumable surgical items");
        var result = await _service.CreateCategoryAsync(request);

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal("Surgical Supplies", result.Name);
        Assert.True(result.IsActive);
        Assert.Equal(0, result.ItemCount);
    }

    [Fact]
    public async Task ToggleCategoryActiveAsync_FlipsFlag()
    {
        var result = await _service.ToggleCategoryActiveAsync(_categoryId);
        Assert.False(result.IsActive);

        var result2 = await _service.ToggleCategoryActiveAsync(_categoryId);
        Assert.True(result2.IsActive);
    }

    [Fact]
    public async Task CreateItemAsync_ReturnsItemInCategory()
    {
        var request = new CreateInventoryItemRequest(_categoryId, "SYR-5ML", "5mL Syringe", "Unit", 50, 0.10m);
        var result = await _service.CreateItemAsync(request);

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(_categoryId, result.CategoryId);
        Assert.Equal("SYR-5ML", result.Code);
        Assert.Equal(0, result.CurrentStock);
        Assert.True(result.IsLowStock); // 0 stock, reorder level 50 → low stock
    }

    [Fact]
    public async Task StockInAsync_IncreasesStock()
    {
        var request = new StockInRequest(_itemId, 50, _pharmacistId, "Monthly delivery");
        var result = await _service.StockInAsync(request);

        Assert.Equal("In", result.Type);
        Assert.Equal(50, result.Quantity);
        Assert.Equal(250, result.StockAfter); // 200 + 50

        var item = await _service.GetItemByIdAsync(_itemId);
        Assert.Equal(250, item!.CurrentStock);
    }

    [Fact]
    public async Task StockOutAsync_DecreasesStock()
    {
        var request = new StockOutRequest(_itemId, 30, _pharmacistId, "Dispensed to ward");
        var result = await _service.StockOutAsync(request);

        Assert.Equal("Out", result.Type);
        Assert.Equal(30, result.Quantity);
        Assert.Equal(170, result.StockAfter); // 200 - 30

        var item = await _service.GetItemByIdAsync(_itemId);
        Assert.Equal(170, item!.CurrentStock);
    }

    [Fact]
    public async Task StockOutAsync_Throws_WhenInsufficientStock()
    {
        var request = new StockOutRequest(_itemId, 999, _pharmacistId);
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.StockOutAsync(request));
    }

    [Fact]
    public async Task AdjustStockAsync_SetsNewStock()
    {
        var request = new AdjustStockRequest(_itemId, 150, _pharmacistId, "Physical count correction");
        var result = await _service.AdjustStockAsync(request);

        Assert.Equal("Adjustment", result.Type);
        Assert.Equal(-50, result.Quantity); // 150 - 200
        Assert.Equal(150, result.StockAfter);

        var item = await _service.GetItemByIdAsync(_itemId);
        Assert.Equal(150, item!.CurrentStock);
    }

    [Fact]
    public async Task GetAllItemsAsync_FiltersLowStock()
    {
        using var ctx = CreateContext();
        var svc = new InventoryService(ctx);

        var lowItem = TestFixtures.CreateInventoryItem(
            categoryId: _categoryId, code: "LOW-001", name: "Low Stock Item",
            reorderLevel: 100, currentStock: 5);
        await ctx.InventoryItems.AddAsync(lowItem);
        await ctx.SaveChangesAsync();

        var lowOnly = await svc.GetAllItemsAsync(lowStockOnly: true);
        Assert.Contains(lowOnly, i => i.Id == lowItem.Id);
        Assert.All(lowOnly, i => Assert.True(i.IsLowStock));
    }

    [Fact]
    public async Task GetTransactionsByItemAsync_ReturnsHistoryInDescendingOrder()
    {
        await _service.StockInAsync(new StockInRequest(_itemId, 10, _pharmacistId));
        await _service.StockOutAsync(new StockOutRequest(_itemId, 5, _pharmacistId));

        var txs = await _service.GetTransactionsByItemAsync(_itemId);

        Assert.Equal(2, txs.Count);
        Assert.True(txs[0].TransactedAtUtc >= txs[1].TransactedAtUtc);
    }
}
