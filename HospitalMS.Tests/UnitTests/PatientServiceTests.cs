using HospitalMS.Business.Models;
using HospitalMS.Business.Repositories;
using HospitalMS.Business.Services;
using HospitalMS.Data.Persistence.Entities;
using Moq;

namespace HospitalMS.Tests.UnitTests;

public sealed class PatientServiceTests : UnitTestBase
{
    private Mock<IRepository<Patient>> _repositoryMock = null!;
    private PatientService _service = null!;

    protected override Task OnInitializeAsync()
    {
        _repositoryMock = CreateRepositoryMock<Patient>();
        _service = new PatientService(_repositoryMock.Object);
        return Task.CompletedTask;
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllPatients()
    {
        var patients = (IReadOnlyList<Patient>)TestFixtures.CreatePatients(3);
        _repositoryMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(patients)
            .Verifiable();

        var result = await _service.GetAllAsync();

        Assert.Equal(3, result.Count);
        VerifyAllMocks();
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenPatientNotFound()
    {
        var id = Guid.NewGuid();
        _repositoryMock
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Patient?)null)
            .Verifiable();

        var result = await _service.GetByIdAsync(id);

        Assert.Null(result);
        VerifyAllMocks();
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsPatient_WhenFound()
    {
        var patient = TestFixtures.CreatePatient(firstName: "Jane", lastName: "Doe");
        _repositoryMock
            .Setup(r => r.GetByIdAsync(patient.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(patient)
            .Verifiable();

        var result = await _service.GetByIdAsync(patient.Id);

        Assert.NotNull(result);
        Assert.Equal("Jane", result.FirstName);
        Assert.Equal("Doe", result.LastName);
        VerifyAllMocks();
    }

    [Fact]
    public async Task CreateAsync_AddsAndReturnsPatient()
    {
        var request = new CreatePatientRequest("MRN-001", "Alice", "Smith", new DateTime(1985, 6, 15), "alice@test.com");
        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Patient>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable();

        var result = await _service.CreateAsync(request);

        Assert.Equal("Alice", result.FirstName);
        Assert.Equal("MRN-001", result.MedicalRecordNumber);
        VerifyAllMocks();
    }

    [Fact]
    public async Task UpdateAsync_ThrowsKeyNotFoundException_WhenPatientNotFound()
    {
        var id = Guid.NewGuid();
        _repositoryMock
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Patient?)null)
            .Verifiable();

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _service.UpdateAsync(id, new UpdatePatientRequest("A", "B", DateTime.Today, "a@b.com")));

        VerifyAllMocks();
    }

    [Fact]
    public async Task DeleteAsync_ThrowsKeyNotFoundException_WhenPatientNotFound()
    {
        var id = Guid.NewGuid();
        _repositoryMock
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Patient?)null)
            .Verifiable();

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.DeleteAsync(id));

        VerifyAllMocks();
    }
}
