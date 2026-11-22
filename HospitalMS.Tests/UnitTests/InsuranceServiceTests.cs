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

    // ── Claims ───────────────────────────────────────────────────────────────

    private async Task<(Guid InvoiceId, Guid PolicyId)> SeedInvoiceWithPolicyAsync(
        string invoiceStatus = InvoiceStatus.Issued, Guid? policyPatientId = null, bool policyActive = true)
    {
        using var ctx = CreateContext();
        var invoice = TestFixtures.CreateInvoice(patientId: _patientId, status: invoiceStatus, totalAmount: 500m);
        var policy = new PatientInsurance
        {
            PatientId   = policyPatientId ?? _patientId,
            ProviderId  = _providerId,
            PolicyNumber = "POL-777",
            IsActive    = policyActive
        };
        await ctx.Invoices.AddAsync(invoice);
        await ctx.PatientInsurances.AddAsync(policy);
        await ctx.SaveChangesAsync();
        return (invoice.Id, policy.Id);
    }

    [Fact]
    public async Task CreateClaimAsync_FilesPendingClaimForIssuedInvoice()
    {
        var (invoiceId, policyId) = await SeedInvoiceWithPolicyAsync();

        var claim = await _service.CreateClaimAsync(new CreateInsuranceClaimRequest(invoiceId, policyId, "CLM-1"));

        Assert.Equal(ClaimStatus.Pending, claim.Status);
        Assert.Equal("POL-777", claim.PolicyNumber);
        Assert.Equal("BlueCross Medical", claim.ProviderName);
    }

    [Fact]
    public async Task CreateClaimAsync_RejectsDraftInvoice()
    {
        var (invoiceId, policyId) = await SeedInvoiceWithPolicyAsync(InvoiceStatus.Draft);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.CreateClaimAsync(new CreateInsuranceClaimRequest(invoiceId, policyId, "CLM-1")));
    }

    [Fact]
    public async Task CreateClaimAsync_RejectsPolicyOfAnotherPatient()
    {
        var (invoiceId, policyId) = await SeedInvoiceWithPolicyAsync(policyPatientId: Guid.NewGuid());

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.CreateClaimAsync(new CreateInsuranceClaimRequest(invoiceId, policyId, "CLM-1")));
    }

    [Fact]
    public async Task CreateClaimAsync_RejectsInactivePolicy()
    {
        var (invoiceId, policyId) = await SeedInvoiceWithPolicyAsync(policyActive: false);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.CreateClaimAsync(new CreateInsuranceClaimRequest(invoiceId, policyId, "CLM-1")));
    }

    [Fact]
    public async Task CreateClaimAsync_RequiresClaimNumber()
    {
        var (invoiceId, policyId) = await SeedInvoiceWithPolicyAsync();

        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.CreateClaimAsync(new CreateInsuranceClaimRequest(invoiceId, policyId, "  ")));
    }

    [Fact]
    public async Task UpdateClaimStatusAsync_ApproveSetsAmountAndResolvedDate()
    {
        var (invoiceId, policyId) = await SeedInvoiceWithPolicyAsync();
        var claim = await _service.CreateClaimAsync(new CreateInsuranceClaimRequest(invoiceId, policyId, "CLM-1"));

        var updated = await _service.UpdateClaimStatusAsync(claim.Id,
            new UpdateClaimStatusRequest(ClaimStatus.Approved, ApprovedAmount: 450m));

        Assert.Equal(ClaimStatus.Approved, updated.Status);
        Assert.Equal(450m, updated.ApprovedAmount);
        Assert.NotNull(updated.ResolvedAtUtc);
    }

    [Theory]
    [InlineData("Lost", null, null)]                   // unknown status
    [InlineData(ClaimStatus.Approved, null, null)]     // approval without amount
    [InlineData(ClaimStatus.Paid, 0, null)]            // zero amount
    [InlineData(ClaimStatus.Rejected, null, " ")]      // rejection without reason
    public async Task UpdateClaimStatusAsync_RejectsInvalidRequests(string status, int? amount, string? reason)
    {
        var (invoiceId, policyId) = await SeedInvoiceWithPolicyAsync();
        var claim = await _service.CreateClaimAsync(new CreateInsuranceClaimRequest(invoiceId, policyId, "CLM-1"));

        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.UpdateClaimStatusAsync(claim.Id, new UpdateClaimStatusRequest(status, amount, reason)));
    }
}
