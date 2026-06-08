using HospitalMS.Business.Services;
using HospitalMS.Data.Persistence.Entities;

namespace HospitalMS.Tests.UnitTests;

public sealed class AnalyticsServiceTests : IntegrationTestBase
{
    private AnalyticsService _service = null!;
    private Guid _patientId;
    private Guid _userId;
    private Guid _wardId;
    private Guid _bedId;

    protected override async Task SeedAsync(HospitalMS.Data.Persistence.HospitalDbContext context)
    {
        var user    = TestFixtures.CreateUser(role: "Administrator");
        var patient = TestFixtures.CreatePatient();
        var ward    = TestFixtures.CreateWard();
        var bed     = TestFixtures.CreateBed(wardId: ward.Id);
        var cat     = TestFixtures.CreateInventoryCategory();

        await context.Users.AddAsync(user);
        await context.Patients.AddAsync(patient);
        await context.Wards.AddAsync(ward);
        await context.Beds.AddAsync(bed);
        await context.InventoryCategories.AddAsync(cat);
        await context.SaveChangesAsync();

        _patientId = patient.Id;
        _userId    = user.Id;
        _wardId    = ward.Id;
        _bedId     = bed.Id;
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _service = new AnalyticsService(Context);
    }

    // ── Dashboard ───────────────────────────────────────────────────────────

    [Fact]
    public async Task GetDashboardSummaryAsync_ReturnsPatientCount()
    {
        var summary = await _service.GetDashboardSummaryAsync();
        Assert.Equal(1, summary.TotalPatients);
    }

    [Fact]
    public async Task GetDashboardSummaryAsync_CountsOccupiedBeds()
    {
        await Context.BedAllocations.AddAsync(new BedAllocation
        {
            Id = Guid.NewGuid(),
            BedId = _bedId,
            PatientId = _patientId,
            AdmittedByUserId = _userId,
            AdmittedAtUtc = DateTime.UtcNow,
            DischargedAtUtc = null
        });
        await Context.SaveChangesAsync();

        var summary = await _service.GetDashboardSummaryAsync();
        Assert.Equal(1, summary.OccupiedBeds);
        Assert.True(summary.OccupancyRate > 0);
    }

    [Fact]
    public async Task GetDashboardSummaryAsync_CountsLowStock()
    {
        var cat  = TestFixtures.CreateInventoryCategory();
        await Context.InventoryCategories.AddAsync(cat);
        var item = TestFixtures.CreateInventoryItem(categoryId: cat.Id,
            currentStock: 5, reorderLevel: 20);
        await Context.InventoryItems.AddAsync(item);
        await Context.SaveChangesAsync();

        var summary = await _service.GetDashboardSummaryAsync();
        Assert.True(summary.LowStockItemCount >= 1);
    }

    // ── Patient Demographics ────────────────────────────────────────────────

    [Fact]
    public async Task GetPatientDemographicsAsync_ReturnsTotalPatients()
    {
        var report = await _service.GetPatientDemographicsAsync();
        Assert.Equal(1, report.TotalPatients);
    }

    [Fact]
    public async Task GetPatientDemographicsAsync_GroupsByGender()
    {
        var report = await _service.GetPatientDemographicsAsync();
        Assert.NotEmpty(report.ByGender);
        Assert.Equal(1, report.ByGender.Sum(g => g.Count));
    }

    [Fact]
    public async Task GetPatientDemographicsAsync_GroupsByAgeGroup()
    {
        var report = await _service.GetPatientDemographicsAsync();
        Assert.NotEmpty(report.ByAgeGroup);
        Assert.Equal(1, report.ByAgeGroup.Sum(g => g.Count));
    }

    // ── Appointments ────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAppointmentReportAsync_ReturnsZeroWhenNoAppointments()
    {
        var report = await _service.GetAppointmentReportAsync();
        Assert.Equal(0, report.TotalAppointments);
        Assert.Empty(report.ByStatus);
    }

    [Fact]
    public async Task GetAppointmentReportAsync_CountsAppointmentsByStatus()
    {
        var apt = TestFixtures.CreateAppointment(patientId: _patientId, doctorUserId: _userId);
        await Context.Appointments.AddAsync(apt);
        await Context.SaveChangesAsync();

        var report = await _service.GetAppointmentReportAsync();
        Assert.Equal(1, report.TotalAppointments);
        Assert.Single(report.ByStatus);
    }

    // ── Revenue ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetRevenueReportAsync_ReturnsZeroWhenNoPayments()
    {
        var report = await _service.GetRevenueReportAsync();
        Assert.Equal(0m, report.TotalRevenue);
        Assert.Empty(report.ByPaymentMethod);
    }

    [Fact]
    public async Task GetRevenueReportAsync_SumsPaymentsAndGroupsByMethod()
    {
        var invoice = TestFixtures.CreateInvoice(patientId: _patientId,
            createdByUserId: _userId, status: "Paid", totalAmount: 200m);
        await Context.Invoices.AddAsync(invoice);
        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            InvoiceId = invoice.Id,
            RecordedByUserId = _userId,
            Amount = 200m,
            Method = PaymentMethod.Cash,
            PaidAtUtc = DateTime.UtcNow
        };
        await Context.Payments.AddAsync(payment);
        await Context.SaveChangesAsync();

        var report = await _service.GetRevenueReportAsync();
        Assert.Equal(200m, report.TotalRevenue);
        Assert.Single(report.ByPaymentMethod);
        Assert.Equal(PaymentMethod.Cash, report.ByPaymentMethod[0].Label);
    }

    // ── Bed Occupancy ───────────────────────────────────────────────────────

    [Fact]
    public async Task GetBedOccupancyReportAsync_ReturnsWardRow()
    {
        var report = await _service.GetBedOccupancyReportAsync();
        Assert.Equal(1, report.TotalBeds);
        Assert.Equal(0, report.OccupiedBeds);
        Assert.Single(report.ByWard);
    }

    // ── Inventory Status ────────────────────────────────────────────────────

    [Fact]
    public async Task GetInventoryStatusReportAsync_ReturnsLowStockItems()
    {
        var cat  = TestFixtures.CreateInventoryCategory();
        await Context.InventoryCategories.AddAsync(cat);
        var item = TestFixtures.CreateInventoryItem(categoryId: cat.Id,
            currentStock: 0, reorderLevel: 10);
        await Context.InventoryItems.AddAsync(item);
        await Context.SaveChangesAsync();

        var report = await _service.GetInventoryStatusReportAsync();
        Assert.True(report.LowStockCount >= 1);
        Assert.Contains(report.LowStockItems, i => i.CurrentStock == 0);
    }
}
