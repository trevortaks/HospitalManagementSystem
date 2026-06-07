using HospitalMS.Business.Repositories;
using HospitalMS.Business.Services;
using HospitalMS.Data.Persistence.Entities;
using Moq;

namespace HospitalMS.Tests.UnitTests;

public sealed class SampleServiceTests : UnitTestBase
{
    [Fact]
    public async Task GetPatientStatusAsync_ReturnsPatientSpecificStatusMessage()
    {
        var patient = TestFixtures.CreatePatient(firstName: "Alice", lastName: "Moyo");
        var repositoryMock = CreateRepositoryMock<Patient>();
        var statusServiceMock = CreateSystemStatusServiceMock();

        repositoryMock
            .Setup(repository => repository.GetByIdAsync(patient.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(patient)
            .Verifiable();

        statusServiceMock
            .Setup(service => service.GetStatus())
            .Returns("Ready for consultation")
            .Verifiable();

        var service = new SamplePatientService(repositoryMock.Object, statusServiceMock.Object);

        var result = await service.GetPatientStatusAsync(patient.Id);

        Assert.Equal("Alice Moyo: Ready for consultation", result);
        VerifyAllMocks();
    }

    [Fact]
    public async Task GetPatientStatusAsync_ThrowsWhenPatientDoesNotExist()
    {
        var patientId = Guid.NewGuid();
        var repositoryMock = CreateRepositoryMock<Patient>();
        var statusServiceMock = CreateSystemStatusServiceMock("Unused status");

        repositoryMock
            .Setup(repository => repository.GetByIdAsync(patientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Patient?)null)
            .Verifiable();

        var service = new SamplePatientService(repositoryMock.Object, statusServiceMock.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.GetPatientStatusAsync(patientId));
        VerifyAllMocks();
    }

    private sealed class SamplePatientService(IRepository<Patient> repository, ISystemStatusService statusService)
    {
        public async Task<string> GetPatientStatusAsync(Guid patientId, CancellationToken cancellationToken = default)
        {
            var patient = await repository.GetByIdAsync(patientId, cancellationToken)
                ?? throw new InvalidOperationException("Patient was not found.");

            return $"{patient.FirstName} {patient.LastName}: {statusService.GetStatus()}";
        }
    }
}
