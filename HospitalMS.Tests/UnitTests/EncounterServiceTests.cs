using HospitalMS.Business.Models;
using HospitalMS.Business.Services;
using HospitalMS.Data.Persistence.Entities;

namespace HospitalMS.Tests.UnitTests;

public sealed class EncounterServiceTests : IntegrationTestBase
{
    private EncounterService _service = null!;

    private Guid _seededPatientId;
    private Guid _seededDoctorId;
    private Guid _seededEncounterId;

    protected override async Task SeedAsync(HospitalMS.Data.Persistence.HospitalDbContext context)
    {
        var patient = TestFixtures.CreatePatient();
        var doctor  = TestFixtures.CreateUser(role: "Doctor");
        await context.Patients.AddAsync(patient);
        await context.Users.AddAsync(doctor);

        var encounter = TestFixtures.CreateEncounter(
            patientId: patient.Id,
            attendingDoctorId: doctor.Id);
        await context.ClinicalEncounters.AddAsync(encounter);
        await context.SaveChangesAsync();

        _seededPatientId  = patient.Id;
        _seededDoctorId   = doctor.Id;
        _seededEncounterId = encounter.Id;
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _service = new EncounterService(Context);
    }

    [Fact]
    public async Task CreateAsync_ReturnsEncounterWithPatientAndDoctorNames()
    {
        var request = new CreateEncounterRequest(
            _seededPatientId,
            _seededDoctorId,
            EncounterType.Outpatient,
            ChiefComplaint: "Chest pain");

        var result = await _service.CreateAsync(request);

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal("Jane Doe", result.PatientName);
        Assert.Equal("Chest pain", result.ChiefComplaint);
        Assert.False(result.IsClosed);
    }

    [Fact]
    public async Task CloseAsync_SetsIsClosedTrue()
    {
        var result = await _service.CloseAsync(_seededEncounterId);

        Assert.True(result.IsClosed);
        Assert.NotNull(result.EndedAtUtc);
    }

    [Fact]
    public async Task CloseAsync_ThrowsInvalidOperation_WhenAlreadyClosed()
    {
        await _service.CloseAsync(_seededEncounterId);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.CloseAsync(_seededEncounterId));
    }

    [Fact]
    public async Task AddDiagnosisAsync_AddsDiagnosis_ToOpenEncounter()
    {
        var request = new AddDiagnosisRequest("J06.9", "Acute upper respiratory infection", "Primary");

        var result = await _service.AddDiagnosisAsync(_seededEncounterId, request);

        Assert.Equal("J06.9", result.IcdCode);
        Assert.Equal("Acute upper respiratory infection", result.Description);
        Assert.Equal("Primary", result.DiagnosisType);
    }

    [Fact]
    public async Task AddDiagnosisAsync_ThrowsInvalidOperation_WhenEncounterClosed()
    {
        await _service.CloseAsync(_seededEncounterId);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.AddDiagnosisAsync(_seededEncounterId,
                new AddDiagnosisRequest("Z00.0", "Routine check", "Primary")));
    }

    [Fact]
    public async Task AddVitalsAsync_AddsVitals_ToOpenEncounter()
    {
        var request = new AddVitalsRequest(
            HeightCm: 170m,
            WeightKg: 70m,
            TemperatureCelsius: 36.6m,
            HeartRateBpm: 72,
            BloodPressureSystolic: 120,
            BloodPressureDiastolic: 80,
            OxygenSaturationPct: 98m);

        var result = await _service.AddVitalsAsync(_seededEncounterId, _seededDoctorId, request);

        Assert.Equal(170m, result.HeightCm);
        Assert.Equal(70m, result.WeightKg);
        Assert.Equal(72, result.HeartRateBpm);
    }

    [Fact]
    public async Task UpdateAsync_ThrowsInvalidOperation_WhenEncounterClosed()
    {
        await _service.CloseAsync(_seededEncounterId);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.UpdateAsync(_seededEncounterId,
                new UpdateEncounterRequest(EncounterType.Outpatient, "New complaint", null, null, null, null, null)));
    }

    [Fact]
    public async Task GetAllAsync_FiltersByPatientId()
    {
        var result = await _service.GetAllAsync(patientId: _seededPatientId);
        Assert.All(result, e => Assert.Equal(_seededPatientId, e.PatientId));
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
    {
        var result = await _service.GetByIdAsync(Guid.NewGuid());
        Assert.Null(result);
    }
}
