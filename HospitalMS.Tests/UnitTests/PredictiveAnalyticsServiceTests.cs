using HospitalMS.Business.Services;
using HospitalMS.Data.Persistence.Entities;

namespace HospitalMS.Tests.UnitTests;

public sealed class PredictiveAnalyticsServiceTests : IntegrationTestBase
{
    private PredictiveAnalyticsService _service = null!;
    private Guid _patientId;
    private Guid _userId;

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
        _service = new PredictiveAnalyticsService(Context);
    }

    // ── Readmission Risk ─────────────────────────────────────────────────────

    [Fact]
    public async Task GetReadmissionRisksAsync_ReturnsEmptyWhenNoEncounters()
    {
        var risks = await _service.GetReadmissionRisksAsync();
        Assert.Empty(risks);
    }

    [Fact]
    public async Task GetReadmissionRisksAsync_ScoresPatientWithRecentEncounter()
    {
        var encounter = new ClinicalEncounter
        {
            Id = Guid.NewGuid(),
            PatientId = _patientId,
            AttendingDoctorId = _userId,
            EncounterType = "General",
            StartedAtUtc = DateTime.UtcNow.AddDays(-5)
        };
        await Context.ClinicalEncounters.AddAsync(encounter);
        await Context.SaveChangesAsync();

        var risks = await _service.GetReadmissionRisksAsync();
        Assert.Single(risks);
        Assert.True(risks[0].RiskScore >= 30);
        Assert.Contains("Encounter within 30 days", risks[0].RiskFactors);
    }

    // ── Bed Demand Forecast ──────────────────────────────────────────────────

    [Fact]
    public async Task GetBedDemandForecastAsync_ReturnsRequestedWeeks()
    {
        var forecasts = await _service.GetBedDemandForecastAsync(3);
        Assert.Equal(3, forecasts.Count);
    }

    [Fact]
    public async Task GetBedDemandForecastAsync_RecommendedBedsIsHigherThanPredicted()
    {
        var forecasts = await _service.GetBedDemandForecastAsync(2);
        foreach (var f in forecasts)
        {
            Assert.True(f.RecommendedAvailableBeds >= f.PredictedAdmissions);
        }
    }

    // ── Low Stock Predictions ────────────────────────────────────────────────

    [Fact]
    public async Task GetLowStockPredictionsAsync_ReturnsEmptyWhenNoItems()
    {
        var predictions = await _service.GetLowStockPredictionsAsync();
        Assert.Empty(predictions);
    }

    [Fact]
    public async Task GetLowStockPredictionsAsync_IncludesOutOfStockItem()
    {
        var cat  = TestFixtures.CreateInventoryCategory();
        await Context.InventoryCategories.AddAsync(cat);
        var item = TestFixtures.CreateInventoryItem(categoryId: cat.Id,
            currentStock: 0, reorderLevel: 50);
        await Context.InventoryItems.AddAsync(item);
        await Context.SaveChangesAsync();

        var predictions = await _service.GetLowStockPredictionsAsync();
        Assert.Contains(predictions, p => p.Urgency == "Out of Stock");
    }

    // ── No-Show Risks ────────────────────────────────────────────────────────

    [Fact]
    public async Task GetNoShowRisksAsync_ReturnsEmptyWhenNoUpcomingAppointments()
    {
        var risks = await _service.GetNoShowRisksAsync();
        Assert.Empty(risks);
    }

    [Fact]
    public async Task GetNoShowRisksAsync_ScoresHighWhenPatientHasMissedHistory()
    {
        var past = TestFixtures.CreateAppointment(patientId: _patientId,
            doctorUserId: _userId, status: AppointmentStatus.NoShow);
        past.ScheduledAtUtc = DateTime.UtcNow.AddDays(-10);
        var upcoming = TestFixtures.CreateAppointment(patientId: _patientId, doctorUserId: _userId);
        upcoming.ScheduledAtUtc = DateTime.UtcNow.AddDays(3);

        await Context.Appointments.AddRangeAsync(past, upcoming);
        await Context.SaveChangesAsync();

        var risks = await _service.GetNoShowRisksAsync();
        Assert.Single(risks);
        Assert.True(risks[0].NoShowProbability >= 40);
    }

    // ── Summary ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetInsightsSummaryAsync_ReturnsStructuredSummary()
    {
        var summary = await _service.GetInsightsSummaryAsync();
        Assert.NotNull(summary);
        Assert.Equal(4, summary.WeeklyBedForecast.Count);
    }
}
