using HospitalMS.Business.Models;
using HospitalMS.Business.Services;
using HospitalMS.Data.Persistence.Entities;

namespace HospitalMS.Tests.UnitTests;

public sealed class LabServiceTests : IntegrationTestBase
{
    private LabService _service = null!;
    private Guid _patientId;
    private Guid _doctorId;
    private Guid _techId;
    private Guid _encounterId;
    private Guid _panelId;

    protected override async Task SeedAsync(HospitalMS.Data.Persistence.HospitalDbContext context)
    {
        var patient   = TestFixtures.CreatePatient();
        var doctor    = TestFixtures.CreateUser(role: "Doctor");
        var tech      = TestFixtures.CreateUser(role: "LabTechnician");
        var encounter = TestFixtures.CreateEncounter(patientId: patient.Id, attendingDoctorId: doctor.Id);
        var panel     = TestFixtures.CreateLabOrderPanel();

        await context.Patients.AddAsync(patient);
        await context.Users.AddRangeAsync(doctor, tech);
        await context.ClinicalEncounters.AddAsync(encounter);
        await context.LabOrderPanels.AddAsync(panel);
        await context.SaveChangesAsync();

        _patientId   = patient.Id;
        _doctorId    = doctor.Id;
        _techId      = tech.Id;
        _encounterId = encounter.Id;
        _panelId     = panel.Id;
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _service = new LabService(Context);
    }

    [Fact]
    public async Task CreatePanelAsync_ReturnsPanelResponse()
    {
        var request = new CreateLabOrderPanelRequest("LFT", "Liver Function Tests", "Biochemistry", true);
        var result = await _service.CreatePanelAsync(request);

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal("LFT", result.Code);
        Assert.Equal("Liver Function Tests", result.Name);
        Assert.Equal("Biochemistry", result.Category);
        Assert.True(result.IsActive);
    }

    [Fact]
    public async Task GetPanelByIdAsync_ReturnsNull_WhenNotFound()
    {
        var result = await _service.GetPanelByIdAsync(Guid.NewGuid());
        Assert.Null(result);
    }

    [Fact]
    public async Task TogglePanelActiveAsync_FlipsActiveFlag()
    {
        var result = await _service.TogglePanelActiveAsync(_panelId);
        Assert.False(result.IsActive);

        var result2 = await _service.TogglePanelActiveAsync(_panelId);
        Assert.True(result2.IsActive);
    }

    [Fact]
    public async Task CreateOrderAsync_ReturnsOrderWithJoinedData()
    {
        var request = new CreateLabOrderRequest(_encounterId, _patientId, _doctorId, _panelId, "Urgent", null);
        var result = await _service.CreateOrderAsync(request);

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal("Jane Doe", result.PatientName);
        Assert.Equal("CBC", result.PanelCode);
        Assert.Equal(LabOrderStatus.Ordered, result.Status);
        Assert.Equal("Urgent", result.Priority);
        Assert.Empty(result.Results);
    }

    [Fact]
    public async Task GetOrderByIdAsync_ReturnsNull_WhenNotFound()
    {
        var result = await _service.GetOrderByIdAsync(Guid.NewGuid());
        Assert.Null(result);
    }

    [Fact]
    public async Task CollectOrderAsync_SetsCollectedStatus()
    {
        using var ctx = CreateContext();
        var order = TestFixtures.CreateLabOrder(
            encounterId: _encounterId, patientId: _patientId,
            orderedByUserId: _doctorId, panelId: _panelId);
        await ctx.LabOrders.AddAsync(order);
        await ctx.SaveChangesAsync();

        var svc = new LabService(ctx);
        var result = await svc.CollectOrderAsync(order.Id);

        Assert.Equal(LabOrderStatus.Collected, result.Status);
        Assert.NotNull(result.CollectedAtUtc);
    }

    [Fact]
    public async Task CollectOrderAsync_Throws_WhenCancelled()
    {
        using var ctx = CreateContext();
        var order = TestFixtures.CreateLabOrder(
            encounterId: _encounterId, patientId: _patientId,
            orderedByUserId: _doctorId, panelId: _panelId,
            status: LabOrderStatus.Cancelled);
        await ctx.LabOrders.AddAsync(order);
        await ctx.SaveChangesAsync();

        var svc = new LabService(ctx);
        await Assert.ThrowsAsync<InvalidOperationException>(() => svc.CollectOrderAsync(order.Id));
    }

    [Fact]
    public async Task AddResultAsync_SetsResultedAndAddsResult()
    {
        using var ctx = CreateContext();
        var order = TestFixtures.CreateLabOrder(
            encounterId: _encounterId, patientId: _patientId,
            orderedByUserId: _doctorId, panelId: _panelId);
        await ctx.LabOrders.AddAsync(order);
        await ctx.SaveChangesAsync();

        var svc = new LabService(ctx);
        var request = new AddLabResultRequest("Haemoglobin", "13.5", _techId, "g/dL", "12–16", LabResultFlag.Normal);
        var result = await svc.AddResultAsync(order.Id, request);

        Assert.Equal(LabOrderStatus.Resulted, result.Status);
        Assert.NotNull(result.ResultedAtUtc);
        Assert.Single(result.Results);
        Assert.Equal("Haemoglobin", result.Results[0].AnalyteName);
        Assert.Equal(LabResultFlag.Normal, result.Results[0].Flag);
    }

    [Fact]
    public async Task CancelOrderAsync_Throws_WhenAlreadyResulted()
    {
        using var ctx = CreateContext();
        var order = TestFixtures.CreateLabOrder(
            encounterId: _encounterId, patientId: _patientId,
            orderedByUserId: _doctorId, panelId: _panelId,
            status: LabOrderStatus.Resulted);
        await ctx.LabOrders.AddAsync(order);
        await ctx.SaveChangesAsync();

        var svc = new LabService(ctx);
        await Assert.ThrowsAsync<InvalidOperationException>(() => svc.CancelOrderAsync(order.Id));
    }
}
