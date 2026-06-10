using HospitalMS.Business.Models;
using HospitalMS.Business.Repositories;
using HospitalMS.Data.Persistence.Entities;

namespace HospitalMS.Business.Services;

public sealed class PatientService(IRepository<Patient> repository) : IPatientService
{
    public async Task<IReadOnlyList<PatientResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var patients = await repository.GetAllAsync(cancellationToken);
        return patients.Select(ToResponse).ToArray();
    }

    public async Task<PatientResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var patient = await repository.GetByIdAsync(id, cancellationToken);
        return patient is null ? null : ToResponse(patient);
    }

    public async Task<PatientResponse> CreateAsync(CreatePatientRequest request, CancellationToken cancellationToken = default)
    {
        var patient = new Patient
        {
            MedicalRecordNumber = request.MedicalRecordNumber,
            FirstName = request.FirstName,
            LastName = request.LastName,
            DateOfBirth = request.DateOfBirth,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            Gender = request.Gender,
            BloodGroup = request.BloodGroup,
            AddressLine1 = request.AddressLine1,
            AddressLine2 = request.AddressLine2,
            City = request.City,
            State = request.State,
            PostalCode = request.PostalCode,
            Country = request.Country,
            EmergencyContactName = request.EmergencyContactName,
            EmergencyContactPhone = request.EmergencyContactPhone,
            EmergencyContactRelationship = request.EmergencyContactRelationship,
            EmergencyContactEmail = request.EmergencyContactEmail,
            Allergies = request.Allergies,
            ChronicConditions = request.ChronicConditions,
            Notes = request.Notes
        };
        await repository.AddAsync(patient, cancellationToken);
        return ToResponse(patient);
    }

    public async Task<PatientResponse> UpdateAsync(Guid id, UpdatePatientRequest request, CancellationToken cancellationToken = default)
    {
        var patient = await repository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Patient '{id}' was not found.");

        patient.FirstName = request.FirstName;
        patient.LastName = request.LastName;
        patient.DateOfBirth = request.DateOfBirth;
        patient.Email = request.Email;
        patient.PhoneNumber = request.PhoneNumber;
        patient.Gender = request.Gender;
        patient.BloodGroup = request.BloodGroup;
        patient.AddressLine1 = request.AddressLine1;
        patient.AddressLine2 = request.AddressLine2;
        patient.City = request.City;
        patient.State = request.State;
        patient.PostalCode = request.PostalCode;
        patient.Country = request.Country;
        patient.EmergencyContactName = request.EmergencyContactName;
        patient.EmergencyContactPhone = request.EmergencyContactPhone;
        patient.EmergencyContactRelationship = request.EmergencyContactRelationship;
        patient.EmergencyContactEmail = request.EmergencyContactEmail;
        patient.Allergies = request.Allergies;
        patient.ChronicConditions = request.ChronicConditions;
        patient.Notes = request.Notes;
        patient.UpdatedAtUtc = DateTime.UtcNow;

        await repository.UpdateAsync(patient, cancellationToken);
        return ToResponse(patient);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var patient = await repository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Patient '{id}' was not found.");
        await repository.DeleteAsync(patient, cancellationToken);
    }

    private static PatientResponse ToResponse(Patient p) => new(
        p.Id,
        p.MedicalRecordNumber,
        p.FirstName,
        p.LastName,
        p.DateOfBirth,
        p.Email,
        p.PhoneNumber,
        p.Gender,
        p.BloodGroup,
        p.AddressLine1,
        p.AddressLine2,
        p.City,
        p.State,
        p.PostalCode,
        p.Country,
        p.EmergencyContactName,
        p.EmergencyContactPhone,
        p.EmergencyContactRelationship,
        p.EmergencyContactEmail,
        p.Allergies,
        p.ChronicConditions,
        p.Notes,
        p.CreatedAtUtc,
        p.UpdatedAtUtc);
}
