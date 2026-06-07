using HospitalMS.Business.Models;
using HospitalMS.Business.Services;
using HospitalMS.Data.Persistence.Entities;

namespace HospitalMS.Tests.UnitTests;

public sealed class PrescriptionServiceTests : IntegrationTestBase
{
    private PrescriptionService _service = null!;
    private Guid _patientId;
    private Guid _doctorId;
    private Guid _medicationId;
    private Guid _encounterId;

    protected override async Task SeedAsync(HospitalMS.Data.Persistence.HospitalDbContext context)
    {
        var patient   = TestFixtures.CreatePatient();
        var doctor    = TestFixtures.CreateUser(role: "Doctor");
        var med       = TestFixtures.CreateMedication();
        var encounter = TestFixtures.CreateEncounter(patientId: patient.Id, attendingDoctorId: doctor.Id);

        await context.Patients.AddAsync(patient);
        await context.Users.AddAsync(doctor);
        await context.Medications.AddAsync(med);
        await context.ClinicalEncounters.AddAsync(encounter);
        await context.SaveChangesAsync();

        _patientId   = patient.Id;
        _doctorId    = doctor.Id;
        _medicationId = med.Id;
        _encounterId  = encounter.Id;
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _service = new PrescriptionService(Context);
    }

    [Fact]
    public async Task CreateAsync_ReturnsPrescriptionWithJoinedData()
    {
        var request = new CreatePrescriptionRequest(
            _encounterId, _patientId, _doctorId, _medicationId,
            "500 mg", "BD", 7, "With food");

        var result = await _service.CreateAsync(request);

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal("Jane Doe", result.PatientName);
        Assert.Equal("Amoxicillin", result.MedicationGenericName);
        Assert.Equal(PrescriptionStatus.Active, result.Status);
        Assert.Equal("500 mg", result.Dose);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
    {
        var result = await _service.GetByIdAsync(Guid.NewGuid());
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllAsync_FiltersByPatientId()
    {
        using var ctx = CreateContext();
        var rx1 = TestFixtures.CreatePrescription(
            patientId: _patientId, encounterId: _encounterId,
            prescribedByUserId: _doctorId, medicationId: _medicationId);
        var rx2 = TestFixtures.CreatePrescription(
            patientId: Guid.NewGuid(), encounterId: _encounterId,
            prescribedByUserId: _doctorId, medicationId: _medicationId);
        await ctx.Prescriptions.AddRangeAsync(rx1, rx2);
        await ctx.SaveChangesAsync();

        var svc = new PrescriptionService(ctx);
        var result = await svc.GetAllAsync(patientId: _patientId);

        Assert.All(result, r => Assert.Equal(_patientId, r.PatientId));
    }

    [Fact]
    public async Task GetAllAsync_FiltersByStatus()
    {
        using var ctx = CreateContext();
        var active = TestFixtures.CreatePrescription(
            patientId: _patientId, encounterId: _encounterId,
            prescribedByUserId: _doctorId, medicationId: _medicationId,
            status: PrescriptionStatus.Active);
        var cancelled = TestFixtures.CreatePrescription(
            patientId: _patientId, encounterId: _encounterId,
            prescribedByUserId: _doctorId, medicationId: _medicationId,
            status: PrescriptionStatus.Cancelled);
        await ctx.Prescriptions.AddRangeAsync(active, cancelled);
        await ctx.SaveChangesAsync();

        var svc = new PrescriptionService(ctx);
        var result = await svc.GetAllAsync(status: PrescriptionStatus.Active);

        Assert.All(result, r => Assert.Equal(PrescriptionStatus.Active, r.Status));
    }

    [Fact]
    public async Task DispenseAsync_SetsDispensedState()
    {
        using var ctx = CreateContext();
        var pharmacist = TestFixtures.CreateUser(role: "Pharmacist");
        await ctx.Users.AddAsync(pharmacist);
        var rx = TestFixtures.CreatePrescription(
            patientId: _patientId, encounterId: _encounterId,
            prescribedByUserId: _doctorId, medicationId: _medicationId);
        await ctx.Prescriptions.AddAsync(rx);
        await ctx.SaveChangesAsync();

        var svc = new PrescriptionService(ctx);
        var result = await svc.DispenseAsync(rx.Id, new DispensePrescriptionRequest(30, pharmacist.Id));

        Assert.Equal(PrescriptionStatus.Dispensed, result.Status);
        Assert.Equal(30, result.QuantityDispensed);
        Assert.NotNull(result.DispensedByName);
    }

    [Fact]
    public async Task DispenseAsync_Throws_WhenNotActive()
    {
        using var ctx = CreateContext();
        var rx = TestFixtures.CreatePrescription(
            patientId: _patientId, encounterId: _encounterId,
            prescribedByUserId: _doctorId, medicationId: _medicationId,
            status: PrescriptionStatus.Cancelled);
        await ctx.Prescriptions.AddAsync(rx);
        await ctx.SaveChangesAsync();

        var svc = new PrescriptionService(ctx);
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            svc.DispenseAsync(rx.Id, new DispensePrescriptionRequest(10, _doctorId)));
    }

    [Fact]
    public async Task CancelAsync_SetsCancelledStatus()
    {
        using var ctx = CreateContext();
        var rx = TestFixtures.CreatePrescription(
            patientId: _patientId, encounterId: _encounterId,
            prescribedByUserId: _doctorId, medicationId: _medicationId);
        await ctx.Prescriptions.AddAsync(rx);
        await ctx.SaveChangesAsync();

        var svc = new PrescriptionService(ctx);
        var result = await svc.CancelAsync(rx.Id);

        Assert.Equal(PrescriptionStatus.Cancelled, result.Status);
    }

    [Fact]
    public async Task CancelAsync_Throws_WhenAlreadyDispensed()
    {
        using var ctx = CreateContext();
        var rx = TestFixtures.CreatePrescription(
            patientId: _patientId, encounterId: _encounterId,
            prescribedByUserId: _doctorId, medicationId: _medicationId,
            status: PrescriptionStatus.Dispensed);
        await ctx.Prescriptions.AddAsync(rx);
        await ctx.SaveChangesAsync();

        var svc = new PrescriptionService(ctx);
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            svc.CancelAsync(rx.Id));
    }
}
