using HospitalMS.Business.Models;
using HospitalMS.Business.Services;
using Microsoft.EntityFrameworkCore;

namespace HospitalMS.Tests.UnitTests;

public sealed class PortalServiceTests : IntegrationTestBase
{
    private PortalService _service = null!;
    private Guid _userId;
    private Guid _patientId;
    private Guid _doctorId;
    private Guid _medicationId;

    protected override async Task SeedAsync(HospitalMS.Data.Persistence.HospitalDbContext context)
    {
        var patient = TestFixtures.CreatePatient();
        var doctor  = TestFixtures.CreateUser(role: "Doctor");
        var med     = TestFixtures.CreateMedication();

        // Patient user linked to the patient record
        var patientUser = TestFixtures.CreateUser(role: "Patient");
        patientUser.LinkedPatientId = patient.Id;

        await context.Patients.AddAsync(patient);
        await context.Users.AddRangeAsync(doctor, patientUser);
        await context.Medications.AddAsync(med);
        await context.SaveChangesAsync();

        _userId     = patientUser.Id;
        _patientId  = patient.Id;
        _doctorId   = doctor.Id;
        _medicationId = med.Id;
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _service = new PortalService(Context);
    }

    [Fact]
    public async Task GetProfileAsync_ReturnsProfile_WhenLinked()
    {
        var result = await _service.GetProfileAsync(_userId);

        Assert.NotNull(result);
        Assert.Equal(_patientId, result.PatientId);
        Assert.Equal("Jane", result.FirstName);
    }

    [Fact]
    public async Task GetProfileAsync_ReturnsNull_WhenNoLinkedPatient()
    {
        using var ctx = CreateContext();
        var unlinkedUser = TestFixtures.CreateUser(role: "Patient");
        await ctx.Users.AddAsync(unlinkedUser);
        await ctx.SaveChangesAsync();

        var svc = new PortalService(ctx);
        var result = await svc.GetProfileAsync(unlinkedUser.Id);

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateProfileAsync_UpdatesDemographics()
    {
        using var ctx = CreateContext();
        var svc = new PortalService(ctx);

        var request = new UpdatePortalProfileRequest(
            "+263771234567", "Female", "O+",
            "1 Main St", "Harare", "00263", "Zimbabwe",
            "Jane Sr", "+263771000000");

        var result = await svc.UpdateProfileAsync(_userId, request);

        Assert.NotNull(result);
        Assert.Equal("+263771234567", result.PhoneNumber);
        Assert.Equal("O+", result.BloodGroup);
        Assert.Equal("Zimbabwe", result.Country);
    }

    [Fact]
    public async Task GetDashboardAsync_ReturnsDashboard_WithCounts()
    {
        using var ctx = CreateContext();
        var encounter = TestFixtures.CreateEncounter(patientId: _patientId, attendingDoctorId: _doctorId);
        var appt = TestFixtures.CreateAppointment(patientId: _patientId, doctorUserId: _doctorId);
        appt.ScheduledAtUtc = DateTime.UtcNow.AddDays(2);
        var rx = TestFixtures.CreatePrescription(
            patientId: _patientId, encounterId: encounter.Id,
            prescribedByUserId: _doctorId, medicationId: _medicationId);

        await ctx.ClinicalEncounters.AddAsync(encounter);
        await ctx.Appointments.AddAsync(appt);
        await ctx.Prescriptions.AddAsync(rx);
        await ctx.SaveChangesAsync();

        var svc = new PortalService(ctx);
        var result = await svc.GetDashboardAsync(_userId);

        Assert.NotNull(result);
        Assert.Equal("Jane", result.Profile.FirstName);
        Assert.Equal(1, result.TotalEncounters);
        Assert.Single(result.UpcomingAppointments);
        Assert.Single(result.ActivePrescriptions);
    }

    [Fact]
    public async Task GetAppointmentsAsync_ReturnsOnlyPatientAppointments()
    {
        using var ctx = CreateContext();
        var myAppt    = TestFixtures.CreateAppointment(patientId: _patientId, doctorUserId: _doctorId);
        var otherAppt = TestFixtures.CreateAppointment(patientId: Guid.NewGuid(), doctorUserId: _doctorId);
        await ctx.Appointments.AddRangeAsync(myAppt, otherAppt);
        await ctx.SaveChangesAsync();

        var svc = new PortalService(ctx);
        var result = await svc.GetAppointmentsAsync(_userId);

        Assert.Single(result);
    }

    [Fact]
    public async Task GetPrescriptionsAsync_ReturnsOnlyPatientPrescriptions()
    {
        using var ctx = CreateContext();
        var enc = TestFixtures.CreateEncounter(patientId: _patientId, attendingDoctorId: _doctorId);
        await ctx.ClinicalEncounters.AddAsync(enc);

        var myRx    = TestFixtures.CreatePrescription(patientId: _patientId, encounterId: enc.Id, prescribedByUserId: _doctorId, medicationId: _medicationId);
        var otherRx = TestFixtures.CreatePrescription(patientId: Guid.NewGuid(), encounterId: enc.Id, prescribedByUserId: _doctorId, medicationId: _medicationId);
        await ctx.Prescriptions.AddRangeAsync(myRx, otherRx);
        await ctx.SaveChangesAsync();

        var svc = new PortalService(ctx);
        var result = await svc.GetPrescriptionsAsync(_userId);

        Assert.Single(result);
    }

    [Fact]
    public async Task GetEncountersAsync_ReturnsOnlyPatientEncounters()
    {
        using var ctx = CreateContext();
        var myEnc    = TestFixtures.CreateEncounter(patientId: _patientId, attendingDoctorId: _doctorId);
        var otherEnc = TestFixtures.CreateEncounter(patientId: Guid.NewGuid(), attendingDoctorId: _doctorId);
        await ctx.ClinicalEncounters.AddRangeAsync(myEnc, otherEnc);
        await ctx.SaveChangesAsync();

        var svc = new PortalService(ctx);
        var result = await svc.GetEncountersAsync(_userId);

        Assert.Single(result);
    }

    [Fact]
    public async Task LogSessionAsync_CreatesSessionRecord()
    {
        using var ctx = CreateContext();
        var svc = new PortalService(ctx);

        await svc.LogSessionAsync(new LogPortalSessionRequest(_userId, _patientId, "10.0.0.1", "Mozilla/5.0"));

        var session = await ctx.PatientPortalSessions
            .FirstOrDefaultAsync(s => s.UserId == _userId && s.PatientId == _patientId);
        Assert.NotNull(session);
        Assert.Equal("10.0.0.1", session.IpAddress);
    }
}
