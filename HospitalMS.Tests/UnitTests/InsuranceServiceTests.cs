using HospitalMS.Business.Models;
using HospitalMS.Business.Services;
using HospitalMS.Data.Persistence.Entities;

namespace HospitalMS.Tests.UnitTests;

public sealed class InsuranceServiceTests : IntegrationTestBase
{
    private InsuranceService _service = null!;
    private Guid _patientId;
    private Guid _providerId;

    protected override async Task SeedAsync(HospitalMS.Data.Persistence.HospitalDbContext context)
    {
        var patient  = TestFixtures.CreatePatient();
        var provider = TestFixtures.CreateInsuranceProvider();

        await context.Patients.AddAsync(patient);
        await context.InsuranceProviders.AddAsync(provider);
        await context.SaveChangesAsync();

        _patientId  = patient.Id;
        _providerId = provider.Id;
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _service = new InsuranceService(Context);
    }

    [Fact]
    public async Task CreateProviderAsync_ReturnsSavedProvider()
    {
        var request = new CreateInsuranceProviderRequest("MediCare Plus", "+1 800 000 0002", "info@medicareplus.test");
        var result = await _service.CreateProviderAsync(request);

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal("MediCare Plus", result.Name);
        Assert.True(result.IsActive);
    }

    [Fact]
    public async Task ToggleProviderActiveAsync_FlipsFlag()
    {
        var result = await _service.ToggleProviderActiveAsync(_providerId);
        Assert.False(result.IsActive);

        var result2 = await _service.ToggleProviderActiveAsync(_providerId);
        Assert.True(result2.IsActive);
    }

    [Fact]
    public async Task AddPatientInsuranceAsync_AddsRecord()
    {
        var request = new AddPatientInsuranceRequest(_patientId, _providerId, "POL-12345", "GRP-001", true);
        var result = await _service.AddPatientInsuranceAsync(request);

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(_patientId, result.PatientId);
        Assert.Equal("POL-12345", result.PolicyNumber);
        Assert.True(result.IsPrimary);
        Assert.Equal("BlueCross Medical", result.ProviderName);
    }

    [Fact]
    public async Task SetPrimaryAsync_ClearsPreviousPrimary()
    {
        using var ctx = CreateContext();
        var svc = new InsuranceService(ctx);

        var pi1 = new PatientInsurance { PatientId = _patientId, ProviderId = _providerId, PolicyNumber = "P1", IsPrimary = true, CreatedAtUtc = DateTime.UtcNow };
        var pi2 = new PatientInsurance { PatientId = _patientId, ProviderId = _providerId, PolicyNumber = "P2", IsPrimary = false, CreatedAtUtc = DateTime.UtcNow };
        await ctx.PatientInsurances.AddRangeAsync(pi1, pi2);
        await ctx.SaveChangesAsync();

        var result = await svc.SetPrimaryAsync(pi2.Id);
        Assert.True(result.IsPrimary);

        var coverage = await svc.GetPatientInsuranceAsync(_patientId);
        Assert.Single(coverage.Where(c => c.IsPrimary));
        Assert.Equal(pi2.Id, coverage.First(c => c.IsPrimary).Id);
    }

    [Fact]
    public async Task GetPatientInsuranceAsync_ReturnsCoverageForPatient()
    {
        using var ctx = CreateContext();
        var svc = new InsuranceService(ctx);

        var pi1 = new PatientInsurance { PatientId = _patientId, ProviderId = _providerId, PolicyNumber = "A1", IsPrimary = true, CreatedAtUtc = DateTime.UtcNow };
        var pi2 = new PatientInsurance { PatientId = Guid.NewGuid(), ProviderId = _providerId, PolicyNumber = "B1", IsPrimary = false, CreatedAtUtc = DateTime.UtcNow };
        await ctx.PatientInsurances.AddRangeAsync(pi1, pi2);
        await ctx.SaveChangesAsync();

        var result = await svc.GetPatientInsuranceAsync(_patientId);
        Assert.All(result, r => Assert.Equal(_patientId, r.PatientId));
    }

    [Fact]
    public async Task DeletePatientInsuranceAsync_RemovesRecord()
    {
        using var ctx = CreateContext();
        var svc = new InsuranceService(ctx);

        var pi = new PatientInsurance { PatientId = _patientId, ProviderId = _providerId, PolicyNumber = "DEL-1", CreatedAtUtc = DateTime.UtcNow };
        await ctx.PatientInsurances.AddAsync(pi);
        await ctx.SaveChangesAsync();

        await svc.DeletePatientInsuranceAsync(pi.Id);
        var remaining = await svc.GetPatientInsuranceAsync(_patientId);
        Assert.Empty(remaining);
    }

    [Fact]
    public async Task GetProviderByIdAsync_ReturnsNull_WhenNotFound()
    {
        var result = await _service.GetProviderByIdAsync(Guid.NewGuid());
        Assert.Null(result);
    }
}
