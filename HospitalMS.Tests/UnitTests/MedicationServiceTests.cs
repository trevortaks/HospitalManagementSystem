using HospitalMS.Business.Models;
using HospitalMS.Business.Services;

namespace HospitalMS.Tests.UnitTests;

public sealed class MedicationServiceTests : IntegrationTestBase
{
    private MedicationService _service = null!;

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _service = new MedicationService(Context);
    }

    [Fact]
    public async Task CreateAsync_ReturnsMedicationWithCorrectFields()
    {
        var request = new CreateMedicationRequest("Ibuprofen", "Tablet", "Nurofen", "400 mg");
        var result = await _service.CreateAsync(request);

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal("Ibuprofen", result.GenericName);
        Assert.Equal("Nurofen", result.BrandName);
        Assert.Equal("Tablet", result.Form);
        Assert.Equal("400 mg", result.Strength);
        Assert.True(result.IsActive);
        Assert.False(result.IsControlled);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
    {
        var result = await _service.GetByIdAsync(Guid.NewGuid());
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsMedication_WhenExists()
    {
        using var ctx = CreateContext();
        var med = TestFixtures.CreateMedication(genericName: "Paracetamol");
        await ctx.Medications.AddAsync(med);
        await ctx.SaveChangesAsync();

        var svc = new MedicationService(ctx);
        var result = await svc.GetByIdAsync(med.Id);

        Assert.NotNull(result);
        Assert.Equal("Paracetamol", result.GenericName);
    }

    [Fact]
    public async Task GetAllAsync_FiltersActiveOnly()
    {
        using var ctx = CreateContext();
        var active = TestFixtures.CreateMedication(genericName: "Active Drug", isActive: true);
        var inactive = TestFixtures.CreateMedication(genericName: "Inactive Drug", isActive: false);
        await ctx.Medications.AddRangeAsync(active, inactive);
        await ctx.SaveChangesAsync();

        var svc = new MedicationService(ctx);
        var result = await svc.GetAllAsync(activeOnly: true);

        Assert.All(result, m => Assert.True(m.IsActive));
        Assert.DoesNotContain(result, m => m.GenericName == "Inactive Drug");
    }

    [Fact]
    public async Task UpdateAsync_ThrowsKeyNotFoundException_WhenNotFound()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _service.UpdateAsync(Guid.NewGuid(), new UpdateMedicationRequest("X", "Tablet")));
    }

    [Fact]
    public async Task UpdateAsync_UpdatesAllFields()
    {
        using var ctx = CreateContext();
        var med = TestFixtures.CreateMedication(genericName: "OldName");
        await ctx.Medications.AddAsync(med);
        await ctx.SaveChangesAsync();

        var svc = new MedicationService(ctx);
        var result = await svc.UpdateAsync(med.Id, new UpdateMedicationRequest("NewName", "Capsule", "BrandX", "200 mg"));

        Assert.Equal("NewName", result.GenericName);
        Assert.Equal("Capsule", result.Form);
        Assert.Equal("BrandX", result.BrandName);
    }

    [Fact]
    public async Task ToggleActiveAsync_FlipsIsActive()
    {
        using var ctx = CreateContext();
        var med = TestFixtures.CreateMedication(isActive: true);
        await ctx.Medications.AddAsync(med);
        await ctx.SaveChangesAsync();

        var svc = new MedicationService(ctx);
        var result = await svc.ToggleActiveAsync(med.Id);

        Assert.False(result.IsActive);

        var toggled = await svc.ToggleActiveAsync(med.Id);
        Assert.True(toggled.IsActive);
    }

    [Fact]
    public async Task ToggleActiveAsync_ThrowsKeyNotFoundException_WhenNotFound()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _service.ToggleActiveAsync(Guid.NewGuid()));
    }
}
