using HospitalMS.Business.Models;
using HospitalMS.Business.Services;
using HospitalMS.Data.Persistence.Entities;

namespace HospitalMS.Tests.UnitTests;

public sealed class SupplyChainServiceTests : IntegrationTestBase
{
    private SupplyChainService _service = null!;

    private Guid _supplierId;
    private Guid _userId;
    private Guid _itemId;

    protected override async Task SeedAsync(HospitalMS.Data.Persistence.HospitalDbContext context)
    {
        var supplier = TestFixtures.CreateSupplier();
        var user     = TestFixtures.CreateUser(role: "Administrator");
        var category = TestFixtures.CreateInventoryCategory();
        var item     = TestFixtures.CreateInventoryItem(categoryId: category.Id, currentStock: 0);

        await context.Suppliers.AddAsync(supplier);
        await context.Users.AddAsync(user);
        await context.InventoryCategories.AddAsync(category);
        await context.InventoryItems.AddAsync(item);
        await context.SaveChangesAsync();

        _supplierId = supplier.Id;
        _userId     = user.Id;
        _itemId     = item.Id;
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _service = new SupplyChainService(Context);
    }

    // ── Supplier tests ────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateSupplierAsync_ReturnsSavedSupplier()
    {
        var request = new CreateSupplierRequest("PharmaCo Ltd", "Bob Smith", "+1 555 999 0001", "bob@pharmaco.test", "1 Main St");
        var result = await _service.CreateSupplierAsync(request);

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal("PharmaCo Ltd", result.Name);
        Assert.Equal("bob@pharmaco.test", result.ContactEmail);
        Assert.True(result.IsActive);
    }

    [Fact]
    public async Task ToggleSupplierActiveAsync_FlipsFlag()
    {
        var result1 = await _service.ToggleSupplierActiveAsync(_supplierId);
        Assert.False(result1.IsActive);

        var result2 = await _service.ToggleSupplierActiveAsync(_supplierId);
        Assert.True(result2.IsActive);
    }

    // ── Purchase Order create tests ────────────────────────────────────────────

    [Fact]
    public async Task CreatePurchaseOrderAsync_ReturnsOrderWithLines()
    {
        var lines = new List<CreatePurchaseOrderLineRequest>
        {
            new(_itemId, 100, 1.50m)
        };
        var request = new CreatePurchaseOrderRequest(_supplierId, _userId, lines, "First order", null);
        var result = await _service.CreatePurchaseOrderAsync(request);

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.StartsWith("PO-", result.OrderNumber);
        Assert.Equal("Draft", result.Status);
        Assert.Single(result.Lines);
        Assert.Equal(150.00m, result.TotalOrderValue);
    }

    [Fact]
    public async Task CreatePurchaseOrderAsync_Throws_WhenNoLines()
    {
        var request = new CreatePurchaseOrderRequest(_supplierId, _userId, [], null, null);
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreatePurchaseOrderAsync(request));
    }

    // ── State machine tests ──────────────────────────────────────────────────

    [Fact]
    public async Task SubmitPurchaseOrderAsync_SetsSubmitted()
    {
        var po = await CreateDraftPoAsync();
        var result = await _service.SubmitPurchaseOrderAsync(po.Id);
        Assert.Equal("Submitted", result.Status);
    }

    [Fact]
    public async Task SubmitPurchaseOrderAsync_Throws_WhenNotDraft()
    {
        var po = await CreateDraftPoAsync();
        await _service.SubmitPurchaseOrderAsync(po.Id);
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.SubmitPurchaseOrderAsync(po.Id));
    }

    [Fact]
    public async Task ApprovePurchaseOrderAsync_SetsApproved()
    {
        var po = await CreateDraftPoAsync();
        await _service.SubmitPurchaseOrderAsync(po.Id);
        var result = await _service.ApprovePurchaseOrderAsync(po.Id);
        Assert.Equal("Approved", result.Status);
    }

    [Fact]
    public async Task CancelPurchaseOrderAsync_SetsCancelled()
    {
        var po = await CreateDraftPoAsync();
        var result = await _service.CancelPurchaseOrderAsync(po.Id);
        Assert.Equal("Cancelled", result.Status);
    }

    [Fact]
    public async Task CancelPurchaseOrderAsync_Throws_WhenReceived()
    {
        var po = await CreateApprovedPoAsync(quantityOrdered: 10);
        var lineId = po.Lines.First().Id;
        var receiveRequest = new ReceiveGoodsRequest(
            po.Id, _userId,
            [new ReceivePOLineRequest(lineId, 10)],
            null);
        await _service.ReceiveGoodsAsync(receiveRequest);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CancelPurchaseOrderAsync(po.Id));
    }

    // ── Goods receipt tests ───────────────────────────────────────────────────

    [Fact]
    public async Task ReceiveGoodsAsync_UpdatesStockAndStatus()
    {
        var po = await CreateApprovedPoAsync(quantityOrdered: 50);
        var lineId = po.Lines.First().Id;

        var request = new ReceiveGoodsRequest(
            po.Id, _userId,
            [new ReceivePOLineRequest(lineId, 50)],
            "Full delivery");
        var receipt = await _service.ReceiveGoodsAsync(request);

        Assert.NotEqual(Guid.Empty, receipt.Id);
        Assert.Single(receipt.Lines);
        Assert.Equal(50, receipt.Lines.First().QuantityReceived);

        var updatedPo = await _service.GetPurchaseOrderByIdAsync(po.Id);
        Assert.Equal("Received", updatedPo!.Status);
        Assert.Equal(50, updatedPo.Lines.First().QuantityReceived);

        // stock should have been incremented
        var item = await Context.InventoryItems.FindAsync(_itemId);
        Assert.Equal(50, item!.CurrentStock);
    }

    [Fact]
    public async Task ReceiveGoodsAsync_SetsPartiallyReceived_WhenUnderAmount()
    {
        var po = await CreateApprovedPoAsync(quantityOrdered: 100);
        var lineId = po.Lines.First().Id;

        var request = new ReceiveGoodsRequest(
            po.Id, _userId,
            [new ReceivePOLineRequest(lineId, 40)],
            "Partial shipment");
        await _service.ReceiveGoodsAsync(request);

        var updatedPo = await _service.GetPurchaseOrderByIdAsync(po.Id);
        Assert.Equal("PartiallyReceived", updatedPo!.Status);
        Assert.Equal(40, updatedPo.Lines.First().QuantityReceived);

        var item = await Context.InventoryItems.FindAsync(_itemId);
        Assert.Equal(40, item!.CurrentStock);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private async Task<HospitalMS.Business.Models.PurchaseOrderResponse> CreateDraftPoAsync(int quantityOrdered = 10)
    {
        var lines = new List<CreatePurchaseOrderLineRequest> { new(_itemId, quantityOrdered, 2.00m) };
        return await _service.CreatePurchaseOrderAsync(
            new CreatePurchaseOrderRequest(_supplierId, _userId, lines, null, null));
    }

    private async Task<HospitalMS.Business.Models.PurchaseOrderResponse> CreateApprovedPoAsync(int quantityOrdered = 10)
    {
        var po = await CreateDraftPoAsync(quantityOrdered);
        await _service.SubmitPurchaseOrderAsync(po.Id);
        return await _service.ApprovePurchaseOrderAsync(po.Id);
    }
}
