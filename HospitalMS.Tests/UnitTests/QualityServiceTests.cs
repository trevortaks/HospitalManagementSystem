using HospitalMS.Business.Models;
using HospitalMS.Business.Services;
using HospitalMS.Data.Persistence.Entities;

namespace HospitalMS.Tests.UnitTests;

public sealed class QualityServiceTests : IntegrationTestBase
{
    private QualityService _service = null!;
    private Guid _userId;
    private Guid _patientId;

    protected override async Task SeedAsync(HospitalMS.Data.Persistence.HospitalDbContext context)
    {
        var user    = TestFixtures.CreateUser(role: "Doctor");
        var patient = TestFixtures.CreatePatient();
        await context.Users.AddAsync(user);
        await context.Patients.AddAsync(patient);
        await context.SaveChangesAsync();
        _userId    = user.Id;
        _patientId = patient.Id;
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _service = new QualityService(Context);
    }

    // ── Incidents ────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateIncidentAsync_PersistsIncident()
    {
        var req = new CreateQualityIncidentRequest(
            "Wrong Medication Administered",
            "Patient received incorrect dosage.",
            QualityIncidentType.AdverseEvent,
            QualityIncidentSeverity.High,
            _userId);

        var result = await _service.CreateIncidentAsync(req);

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal("Wrong Medication Administered", result.Title);
        Assert.Equal(QualityIncidentStatus.Open, result.Status);
    }

    [Fact]
    public async Task GetIncidentsAsync_FiltersOnStatus()
    {
        var req = new CreateQualityIncidentRequest("Test", "Desc",
            QualityIncidentType.NearMiss, QualityIncidentSeverity.Low, _userId);
        await _service.CreateIncidentAsync(req);

        var open   = await _service.GetIncidentsAsync(status: QualityIncidentStatus.Open);
        var closed = await _service.GetIncidentsAsync(status: QualityIncidentStatus.Closed);

        Assert.Single(open);
        Assert.Empty(closed);
    }

    [Fact]
    public async Task AssignIncidentAsync_SetsStatusToUnderInvestigation()
    {
        var created = await _service.CreateIncidentAsync(new CreateQualityIncidentRequest(
            "T", "D", QualityIncidentType.Complaint, QualityIncidentSeverity.Moderate, _userId));

        var assignee = TestFixtures.CreateUser(role: "Doctor");
        await Context.Users.AddAsync(assignee);
        await Context.SaveChangesAsync();

        var result = await _service.AssignIncidentAsync(
            created.Id, new AssignQualityIncidentRequest(assignee.Id));

        Assert.Equal(QualityIncidentStatus.UnderInvestigation, result.Status);
        Assert.Equal(assignee.Id, result.AssignedToUserId);
    }

    [Fact]
    public async Task ResolveIncidentAsync_SetsResolvedStatus()
    {
        var created = await _service.CreateIncidentAsync(new CreateQualityIncidentRequest(
            "T", "D", QualityIncidentType.Suggestion, QualityIncidentSeverity.Low, _userId));

        var result = await _service.ResolveIncidentAsync(created.Id, new ResolveQualityIncidentRequest());

        Assert.Equal(QualityIncidentStatus.Resolved, result.Status);
        Assert.NotNull(result.ResolvedAtUtc);
    }

    [Fact]
    public async Task ResolveIncidentAsync_Throws_WhenAlreadyResolved()
    {
        var created = await _service.CreateIncidentAsync(new CreateQualityIncidentRequest(
            "T", "D", QualityIncidentType.AdverseEvent, QualityIncidentSeverity.High, _userId));
        await _service.ResolveIncidentAsync(created.Id, new ResolveQualityIncidentRequest());

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.ResolveIncidentAsync(created.Id, new ResolveQualityIncidentRequest()));
    }

    [Fact]
    public async Task CloseIncidentAsync_SetsClosedStatus()
    {
        var created = await _service.CreateIncidentAsync(new CreateQualityIncidentRequest(
            "T", "D", QualityIncidentType.NearMiss, QualityIncidentSeverity.Low, _userId));

        var result = await _service.CloseIncidentAsync(created.Id);
        Assert.Equal(QualityIncidentStatus.Closed, result.Status);
    }

    [Fact]
    public async Task CloseIncidentAsync_Throws_WhenAlreadyClosed()
    {
        var created = await _service.CreateIncidentAsync(new CreateQualityIncidentRequest(
            "T", "D", QualityIncidentType.Complaint, QualityIncidentSeverity.Low, _userId));
        await _service.CloseIncidentAsync(created.Id);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.CloseIncidentAsync(created.Id));
    }

    // ── Feedback ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task SubmitFeedbackAsync_PersistsFeedback()
    {
        var req    = new CreatePatientFeedbackRequest(_patientId, 4, StaffRating: 5, Comments: "Great service");
        var result = await _service.SubmitFeedbackAsync(req);

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(4, result.OverallRating);
        Assert.Equal(5, result.StaffRating);
    }

    [Fact]
    public async Task SubmitFeedbackAsync_Throws_WhenRatingOutOfRange()
    {
        var req = new CreatePatientFeedbackRequest(_patientId, 6);
        await Assert.ThrowsAsync<ArgumentException>(() => _service.SubmitFeedbackAsync(req));
    }

    // ── Summary ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetSummaryAsync_ReturnsAggregates()
    {
        await _service.CreateIncidentAsync(new CreateQualityIncidentRequest(
            "A", "D", QualityIncidentType.AdverseEvent, QualityIncidentSeverity.Critical, _userId));
        await _service.SubmitFeedbackAsync(new CreatePatientFeedbackRequest(_patientId, 5));

        var summary = await _service.GetSummaryAsync();

        Assert.Equal(1, summary.OpenIncidents);
        Assert.Equal(1, summary.TotalFeedback);
        Assert.Equal(5.0, summary.AverageOverallRating);
        Assert.NotEmpty(summary.IncidentsByType);
    }
}
