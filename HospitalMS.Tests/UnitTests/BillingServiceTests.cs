using HospitalMS.Business.Models;
using HospitalMS.Business.Services;
using HospitalMS.Data.Persistence.Entities;

namespace HospitalMS.Tests.UnitTests;

public sealed class BillingServiceTests : IntegrationTestBase
{
    private BillingService _service = null!;
    private Guid _patientId;
    private Guid _accountsManagerId;
    private Guid _chargeItemId;

    protected override async Task SeedAsync(HospitalMS.Data.Persistence.HospitalDbContext context)
    {
        var patient        = TestFixtures.CreatePatient();
        var accountsManager = TestFixtures.CreateUser(role: "AccountsManager");
        var chargeItem     = TestFixtures.CreateChargeItem();

        await context.Patients.AddAsync(patient);
        await context.Users.AddAsync(accountsManager);
        await context.ChargeItems.AddAsync(chargeItem);
        await context.SaveChangesAsync();

        _patientId        = patient.Id;
        _accountsManagerId = accountsManager.Id;
        _chargeItemId     = chargeItem.Id;
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _service = new BillingService(Context);
    }

    [Fact]
    public async Task CreateChargeItemAsync_ReturnsSavedItem()
    {
        var request = new CreateChargeItemRequest("LAB-CBC", "Complete Blood Count", "Lab", 30.00m);
        var result = await _service.CreateChargeItemAsync(request);

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal("LAB-CBC", result.Code);
        Assert.Equal(30.00m, result.UnitPrice);
        Assert.True(result.IsActive);
    }

    [Fact]
    public async Task ToggleChargeItemActiveAsync_FlipsFlag()
    {
        var result = await _service.ToggleChargeItemActiveAsync(_chargeItemId);
        Assert.False(result.IsActive);

        var result2 = await _service.ToggleChargeItemActiveAsync(_chargeItemId);
        Assert.True(result2.IsActive);
    }

    [Fact]
    public async Task CreateInvoiceAsync_ReturnsDraftWithGeneratedNumber()
    {
        var request = new CreateInvoiceRequest(_patientId, _accountsManagerId);
        var result = await _service.CreateInvoiceAsync(request);

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.StartsWith("INV-", result.InvoiceNumber);
        Assert.Equal(InvoiceStatus.Draft, result.Status);
        Assert.Equal(0m, result.TotalAmount);
        Assert.Empty(result.LineItems);
    }

    [Fact]
    public async Task AddLineItemAsync_RecalculatesTotals()
    {
        var invoice = await _service.CreateInvoiceAsync(new CreateInvoiceRequest(_patientId, _accountsManagerId));
        var result = await _service.AddLineItemAsync(invoice.Id, new AddLineItemRequest(_chargeItemId, 2));

        Assert.Single(result.LineItems);
        Assert.Equal(100.00m, result.SubtotalAmount); // 50.00 * 2
        Assert.Equal(100.00m, result.TotalAmount);
        Assert.Equal(100.00m, result.LineItems[0].TotalPrice);
    }

    [Fact]
    public async Task IssueInvoiceAsync_TransitionsDraftToIssued()
    {
        var invoice = await _service.CreateInvoiceAsync(new CreateInvoiceRequest(_patientId, _accountsManagerId));
        await _service.AddLineItemAsync(invoice.Id, new AddLineItemRequest(_chargeItemId, 1));

        var result = await _service.IssueInvoiceAsync(invoice.Id);
        Assert.Equal(InvoiceStatus.Issued, result.Status);
    }

    [Fact]
    public async Task IssueInvoiceAsync_Throws_WhenNoLineItems()
    {
        var invoice = await _service.CreateInvoiceAsync(new CreateInvoiceRequest(_patientId, _accountsManagerId));
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.IssueInvoiceAsync(invoice.Id));
    }

    [Fact]
    public async Task RecordPaymentAsync_UpdatesPaidAmountAndStatus()
    {
        using var ctx = CreateContext();
        var svc = new BillingService(ctx);

        var invoice = await svc.CreateInvoiceAsync(new CreateInvoiceRequest(_patientId, _accountsManagerId));
        await svc.AddLineItemAsync(invoice.Id, new AddLineItemRequest(_chargeItemId, 1));
        await svc.IssueInvoiceAsync(invoice.Id);

        var result = await svc.RecordPaymentAsync(new RecordPaymentRequest(invoice.Id, 50.00m, PaymentMethod.Cash, _accountsManagerId));
        Assert.Equal(50.00m, result.PaidAmount);
        Assert.Equal(InvoiceStatus.Paid, result.Status);
        Assert.Single(result.Payments);
    }

    [Fact]
    public async Task RecordPaymentAsync_SetsPartiallyPaid_WhenUnderAmount()
    {
        using var ctx = CreateContext();
        var svc = new BillingService(ctx);

        var ci2 = TestFixtures.CreateChargeItem(code: "SVC-100", unitPrice: 100m);
        await ctx.ChargeItems.AddAsync(ci2);
        await ctx.SaveChangesAsync();

        var invoice = await svc.CreateInvoiceAsync(new CreateInvoiceRequest(_patientId, _accountsManagerId));
        await svc.AddLineItemAsync(invoice.Id, new AddLineItemRequest(ci2.Id, 1));
        await svc.IssueInvoiceAsync(invoice.Id);

        var result = await svc.RecordPaymentAsync(new RecordPaymentRequest(invoice.Id, 40.00m, PaymentMethod.Card, _accountsManagerId));
        Assert.Equal(InvoiceStatus.PartiallyPaid, result.Status);
        Assert.Equal(60.00m, result.BalanceDue);
    }

    [Fact]
    public async Task VoidInvoiceAsync_Throws_WhenAlreadyPaid()
    {
        using var ctx = CreateContext();
        var svc = new BillingService(ctx);

        var invoice = await svc.CreateInvoiceAsync(new CreateInvoiceRequest(_patientId, _accountsManagerId));
        await svc.AddLineItemAsync(invoice.Id, new AddLineItemRequest(_chargeItemId, 1));
        await svc.IssueInvoiceAsync(invoice.Id);
        await svc.RecordPaymentAsync(new RecordPaymentRequest(invoice.Id, 50.00m, PaymentMethod.Cash, _accountsManagerId));

        await Assert.ThrowsAsync<InvalidOperationException>(() => svc.VoidInvoiceAsync(invoice.Id));
    }
}
