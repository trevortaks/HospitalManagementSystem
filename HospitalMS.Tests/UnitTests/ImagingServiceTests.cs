using HospitalMS.Business.Models;
using HospitalMS.Business.Services;
using HospitalMS.Data.Persistence.Entities;

namespace HospitalMS.Tests.UnitTests;

public sealed class ImagingServiceTests : IntegrationTestBase
{
    private ImagingService _service = null!;
    private Guid _patientId;
    private Guid _doctorId;
    private Guid _radiologistId;
    private Guid _encounterId;

    protected override async Task SeedAsync(HospitalMS.Data.Persistence.HospitalDbContext context)
    {
        var patient     = TestFixtures.CreatePatient();
        var doctor      = TestFixtures.CreateUser(role: "Doctor");
        var radiologist = TestFixtures.CreateUser(role: "Radiologist");
        var encounter   = TestFixtures.CreateEncounter(patientId: patient.Id, attendingDoctorId: doctor.Id);

        await context.Patients.AddAsync(patient);
        await context.Users.AddRangeAsync(doctor, radiologist);
        await context.ClinicalEncounters.AddAsync(encounter);
        await context.SaveChangesAsync();

        _patientId     = patient.Id;
        _doctorId      = doctor.Id;
        _radiologistId = radiologist.Id;
        _encounterId   = encounter.Id;
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _service = new ImagingService(Context);
    }

    [Fact]
    public async Task CreateRequestAsync_ReturnsResponseWithJoinedData()
    {
        var request = new CreateImagingRequestRequest(
            _encounterId, _patientId, _doctorId,
            ImagingModality.CT, "Abdomen", "Suspected appendicitis", LabOrderPriority.Urgent);

        var result = await _service.CreateRequestAsync(request);

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal("Jane Doe", result.PatientName);
        Assert.Equal(ImagingModality.CT, result.Modality);
        Assert.Equal(ImagingRequestStatus.Requested, result.Status);
        Assert.Equal("Urgent", result.Priority);
        Assert.Null(result.Report);
    }

    [Fact]
    public async Task GetRequestByIdAsync_ReturnsNull_WhenNotFound()
    {
        var result = await _service.GetRequestByIdAsync(Guid.NewGuid());
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateStatusAsync_ChangesStatus()
    {
        using var ctx = CreateContext();
        var req = TestFixtures.CreateImagingRequest(
            encounterId: _encounterId, patientId: _patientId,
            requestedByUserId: _doctorId);
        await ctx.ImagingRequests.AddAsync(req);
        await ctx.SaveChangesAsync();

        var svc = new ImagingService(ctx);
        var result = await svc.UpdateStatusAsync(req.Id, ImagingRequestStatus.Scheduled);

        Assert.Equal(ImagingRequestStatus.Scheduled, result.Status);
    }

    [Fact]
    public async Task CancelRequestAsync_SetsCancelledStatus()
    {
        using var ctx = CreateContext();
        var req = TestFixtures.CreateImagingRequest(
            encounterId: _encounterId, patientId: _patientId,
            requestedByUserId: _doctorId);
        await ctx.ImagingRequests.AddAsync(req);
        await ctx.SaveChangesAsync();

        var svc = new ImagingService(ctx);
        var result = await svc.CancelRequestAsync(req.Id);

        Assert.Equal(ImagingRequestStatus.Cancelled, result.Status);
    }

    [Fact]
    public async Task CancelRequestAsync_Throws_WhenAlreadyReported()
    {
        using var ctx = CreateContext();
        var req = TestFixtures.CreateImagingRequest(
            encounterId: _encounterId, patientId: _patientId,
            requestedByUserId: _doctorId,
            status: ImagingRequestStatus.Reported);
        await ctx.ImagingRequests.AddAsync(req);
        await ctx.SaveChangesAsync();

        var svc = new ImagingService(ctx);
        await Assert.ThrowsAsync<InvalidOperationException>(() => svc.CancelRequestAsync(req.Id));
    }

    [Fact]
    public async Task CreateReportAsync_SetsReportedAndAttachesReport()
    {
        using var ctx = CreateContext();
        var req = TestFixtures.CreateImagingRequest(
            encounterId: _encounterId, patientId: _patientId,
            requestedByUserId: _doctorId);
        await ctx.ImagingRequests.AddAsync(req);
        await ctx.SaveChangesAsync();

        var svc = new ImagingService(ctx);
        var reportReq = new CreateImagingReportRequest(_radiologistId, "No acute abnormality.", "Normal chest X-ray.", null);
        var result = await svc.CreateReportAsync(req.Id, reportReq);

        Assert.Equal(ImagingRequestStatus.Reported, result.Status);
        Assert.NotNull(result.Report);
        Assert.Equal("No acute abnormality.", result.Report.ReportText);
        Assert.Equal("Normal chest X-ray.", result.Report.Impression);
    }

    [Fact]
    public async Task CreateReportAsync_Throws_OnDuplicateReport()
    {
        using var ctx = CreateContext();
        var req = TestFixtures.CreateImagingRequest(
            encounterId: _encounterId, patientId: _patientId,
            requestedByUserId: _doctorId);
        await ctx.ImagingRequests.AddAsync(req);

        var existingReport = TestFixtures.CreateImagingReport(
            requestId: req.Id, radiologyUserId: _radiologistId);
        await ctx.ImagingReports.AddAsync(existingReport);
        await ctx.SaveChangesAsync();

        var svc = new ImagingService(ctx);
        var reportReq = new CreateImagingReportRequest(_radiologistId, "Second report attempt.", null, null);
        await Assert.ThrowsAsync<InvalidOperationException>(() => svc.CreateReportAsync(req.Id, reportReq));
    }

    [Fact]
    public async Task GetAllRequestsAsync_FiltersByPatientId()
    {
        using var ctx = CreateContext();
        var req1 = TestFixtures.CreateImagingRequest(
            encounterId: _encounterId, patientId: _patientId, requestedByUserId: _doctorId);
        var req2 = TestFixtures.CreateImagingRequest(
            encounterId: _encounterId, patientId: Guid.NewGuid(), requestedByUserId: _doctorId);
        await ctx.ImagingRequests.AddRangeAsync(req1, req2);
        await ctx.SaveChangesAsync();

        var svc = new ImagingService(ctx);
        var result = await svc.GetAllRequestsAsync(patientId: _patientId);

        Assert.All(result, r => Assert.Equal(_patientId, r.PatientId));
    }
}
