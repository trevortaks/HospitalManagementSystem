using HospitalMS.Data.Persistence;
using HospitalMS.Data.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace HospitalMS.Tests.IntegrationTests;

public sealed class SampleRepositoryTests : IntegrationTestBase
{
    protected override async Task SeedAsync(HospitalDbContext context)
    {
        context.Patients.Add(TestFixtures.CreatePatient(
            medicalRecordNumber: "MRN-SEEDED-001",
            firstName: "Seeded",
            lastName: "Patient"));
        await context.SaveChangesAsync();
    }

    [Fact]
    public async Task AddAsync_PersistsPatientToInMemoryDatabase()
    {
        var patient = TestFixtures.CreatePatient(firstName: "Tariro", lastName: "Sibanda");

        await using (var writeContext = CreateContext())
        {
            var repository = new SamplePatientRepository(writeContext);
            await repository.AddAsync(patient);
        }

        await using var readContext = CreateContext();
        var persistedPatient = await readContext.Patients.SingleAsync(saved => saved.Id == patient.Id);

        Assert.Equal(patient.MedicalRecordNumber, persistedPatient.MedicalRecordNumber);
        Assert.Equal("Tariro", persistedPatient.FirstName);
    }

    [Fact]
    public async Task FindByMedicalRecordNumberAsync_ReturnsSeededPatient()
    {
        await using var readContext = CreateContext();
        var repository = new SamplePatientRepository(readContext);

        var patient = await repository.FindByMedicalRecordNumberAsync("MRN-SEEDED-001");

        Assert.NotNull(patient);
        Assert.Equal("Seeded", patient!.FirstName);
    }

    private sealed class SamplePatientRepository(HospitalDbContext context)
    {
        public async Task AddAsync(Patient patient, CancellationToken cancellationToken = default)
        {
            await context.Patients.AddAsync(patient, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
        }

        public Task<Patient?> FindByMedicalRecordNumberAsync(string medicalRecordNumber, CancellationToken cancellationToken = default)
            => context.Patients.SingleOrDefaultAsync(
                patient => patient.MedicalRecordNumber == medicalRecordNumber,
                cancellationToken);
    }
}
