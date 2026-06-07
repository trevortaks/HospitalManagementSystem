using HospitalMS.Business.Models;
using HospitalMS.Business.Services;
using HospitalMS.Data.Persistence.Entities;

namespace HospitalMS.Tests.UnitTests;

public sealed class AppointmentServiceTests : IntegrationTestBase
{
    private AppointmentService _service = null!;

    protected override async Task SeedAsync(HospitalMS.Data.Persistence.HospitalDbContext context)
    {
        var patient = TestFixtures.CreatePatient();
        var doctor  = TestFixtures.CreateUser(role: "Doctor");
        await context.Patients.AddAsync(patient);
        await context.Users.AddAsync(doctor);
        await context.SaveChangesAsync();

        _seededPatientId = patient.Id;
        _seededDoctorId  = doctor.Id;
    }

    private Guid _seededPatientId;
    private Guid _seededDoctorId;

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _service = new AppointmentService(Context);
    }

    [Fact]
    public async Task CreateAsync_ReturnsAppointmentWithPatientAndDoctorNames()
    {
        var request = new CreateAppointmentRequest(
            _seededPatientId,
            _seededDoctorId,
            DateTime.UtcNow.AddDays(1));

        var result = await _service.CreateAsync(request);

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal("Jane Doe", result.PatientName);
        Assert.Equal(AppointmentStatus.Scheduled, result.Status);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
    {
        var result = await _service.GetByIdAsync(Guid.NewGuid());
        Assert.Null(result);
    }

    [Fact]
    public async Task CancelAsync_SetsStatusToCancelled()
    {
        using var ctx = CreateContext();
        var appt = TestFixtures.CreateAppointment(
            patientId: _seededPatientId,
            doctorUserId: _seededDoctorId);
        await ctx.Appointments.AddAsync(appt);
        await ctx.SaveChangesAsync();

        var svc = new AppointmentService(ctx);
        var result = await svc.CancelAsync(appt.Id, new CancelAppointmentRequest("Patient request"));

        Assert.Equal(AppointmentStatus.Cancelled, result.Status);
        Assert.Equal("Patient request", result.CancelledReason);
    }

    [Fact]
    public async Task UpdateAsync_ThrowsKeyNotFoundException_WhenNotFound()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _service.UpdateAsync(Guid.NewGuid(),
                new UpdateAppointmentRequest(DateTime.UtcNow.AddDays(2), 30, AppointmentType.General, null, null)));
    }

    [Fact]
    public async Task GetAllAsync_FiltersByPatientId()
    {
        using var ctx = CreateContext();
        var otherPatient = TestFixtures.CreateUser(role: "Patient");
        await ctx.Users.AddAsync(otherPatient);

        var appt1 = TestFixtures.CreateAppointment(patientId: _seededPatientId, doctorUserId: _seededDoctorId);
        var appt2 = TestFixtures.CreateAppointment(patientId: Guid.NewGuid(), doctorUserId: _seededDoctorId);
        await ctx.Appointments.AddRangeAsync(appt1, appt2);
        await ctx.SaveChangesAsync();

        var svc = new AppointmentService(ctx);
        var result = await svc.GetAllAsync(patientId: _seededPatientId);

        Assert.All(result, a => Assert.Equal(_seededPatientId, a.PatientId));
    }
}
