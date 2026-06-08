using HospitalMS.Business.Models;
using HospitalMS.Business.Services;
using HospitalMS.Data.Persistence.Entities;

namespace HospitalMS.Tests.UnitTests;

public sealed class BedManagementServiceTests : IntegrationTestBase
{
    private BedManagementService _service = null!;
    private Guid _wardId;
    private Guid _bedId;
    private Guid _patientId;
    private Guid _nurseId;

    protected override async Task SeedAsync(HospitalMS.Data.Persistence.HospitalDbContext context)
    {
        var ward    = TestFixtures.CreateWard(name: "Test Ward", totalBeds: 4);
        var bed     = TestFixtures.CreateBed(wardId: ward.Id, bedNumber: "T1", status: "Available");
        var patient = TestFixtures.CreatePatient();
        var nurse   = TestFixtures.CreateUser(role: "Nurse");

        await context.Wards.AddAsync(ward);
        await context.Beds.AddAsync(bed);
        await context.Patients.AddAsync(patient);
        await context.Users.AddAsync(nurse);
        await context.SaveChangesAsync();

        _wardId    = ward.Id;
        _bedId     = bed.Id;
        _patientId = patient.Id;
        _nurseId   = nurse.Id;
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _service = new BedManagementService(Context);
    }

    [Fact]
    public async Task CreateWardAsync_ReturnsSavedWard()
    {
        var request = new CreateWardRequest("ICU North", "ICU", 8, 2);
        var result = await _service.CreateWardAsync(request);

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal("ICU North", result.Name);
        Assert.Equal("ICU", result.WardType);
        Assert.Equal(8, result.TotalBeds);
        Assert.True(result.IsActive);
    }

    [Fact]
    public async Task ToggleWardActiveAsync_FlipsFlag()
    {
        var result = await _service.ToggleWardActiveAsync(_wardId);
        Assert.False(result.IsActive);

        var result2 = await _service.ToggleWardActiveAsync(_wardId);
        Assert.True(result2.IsActive);
    }

    [Fact]
    public async Task CreateBedAsync_ReturnsBedInWard()
    {
        var request = new CreateBedRequest(_wardId, "T2", "Isolation");
        var result = await _service.CreateBedAsync(request);

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(_wardId, result.WardId);
        Assert.Equal("T2", result.BedNumber);
        Assert.Equal("Available", result.Status);
    }

    [Fact]
    public async Task AllocateBedAsync_SetsOccupied()
    {
        var request = new AllocateBedRequest(_bedId, _patientId, _nurseId);
        var result = await _service.AllocateBedAsync(request);

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(_bedId, result.BedId);
        Assert.Equal(_patientId, result.PatientId);
        Assert.Null(result.DischargedAtUtc);

        var bed = await _service.GetBedByIdAsync(_bedId);
        Assert.Equal("Occupied", bed!.Status);
    }

    [Fact]
    public async Task AllocateBedAsync_Throws_WhenBedNotAvailable()
    {
        using var ctx = CreateContext();
        var svc = new BedManagementService(ctx);

        var bed2 = TestFixtures.CreateBed(wardId: _wardId, bedNumber: "T3", status: "OutOfService");
        await ctx.Beds.AddAsync(bed2);
        await ctx.SaveChangesAsync();

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => svc.AllocateBedAsync(new AllocateBedRequest(bed2.Id, _patientId, _nurseId)));
    }

    [Fact]
    public async Task AllocateBedAsync_Throws_WhenAlreadyOccupied()
    {
        using var ctx = CreateContext();
        var svc = new BedManagementService(ctx);

        await svc.AllocateBedAsync(new AllocateBedRequest(_bedId, _patientId, _nurseId));

        var patient2 = TestFixtures.CreatePatient(firstName: "Other");
        await ctx.Patients.AddAsync(patient2);
        await ctx.SaveChangesAsync();

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => svc.AllocateBedAsync(new AllocateBedRequest(_bedId, patient2.Id, _nurseId)));
    }

    [Fact]
    public async Task DischargePatientAsync_SetsCleaningStatus()
    {
        using var ctx = CreateContext();
        var svc = new BedManagementService(ctx);

        var allocation = await svc.AllocateBedAsync(new AllocateBedRequest(_bedId, _patientId, _nurseId));

        var result = await svc.DischargePatientAsync(
            new DischargePatientRequest(allocation.Id, _nurseId, "Recovered"));

        Assert.NotNull(result.DischargedAtUtc);
        Assert.Equal("Recovered", result.DischargeReason);

        var bed = await svc.GetBedByIdAsync(_bedId);
        Assert.Equal("Cleaning", bed!.Status);
    }

    [Fact]
    public async Task DischargePatientAsync_Throws_WhenAlreadyDischarged()
    {
        using var ctx = CreateContext();
        var svc = new BedManagementService(ctx);

        var allocation = await svc.AllocateBedAsync(new AllocateBedRequest(_bedId, _patientId, _nurseId));
        await svc.DischargePatientAsync(new DischargePatientRequest(allocation.Id, _nurseId));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => svc.DischargePatientAsync(new DischargePatientRequest(allocation.Id, _nurseId)));
    }

    [Fact]
    public async Task GetBedAvailabilityAsync_ReturnsCounts()
    {
        using var ctx = CreateContext();
        var svc = new BedManagementService(ctx);

        var bed2 = TestFixtures.CreateBed(wardId: _wardId, bedNumber: "T4", status: "Occupied");
        await ctx.Beds.AddAsync(bed2);
        await ctx.SaveChangesAsync();

        var result = await svc.GetBedAvailabilityAsync();

        Assert.True(result.TotalBeds >= 2);
        Assert.True(result.OccupiedBeds >= 1);
        Assert.True(result.OccupancyRate > 0);
    }

    [Fact]
    public async Task GetActiveAllocationsAsync_FiltersActiveOnly()
    {
        using var ctx = CreateContext();
        var svc = new BedManagementService(ctx);

        var allocation = await svc.AllocateBedAsync(new AllocateBedRequest(_bedId, _patientId, _nurseId));
        await svc.DischargePatientAsync(new DischargePatientRequest(allocation.Id, _nurseId));

        var active = await svc.GetActiveAllocationsAsync();
        Assert.DoesNotContain(active, a => a.Id == allocation.Id);
    }
}
