using System.Threading.Tasks;
using System.Collections.Generic;
using HospitalManagementSystem.Application.Interfaces;
using HospitalManagementSystem.Application.DTOs;
using HospitalManagementSystem.Domain.Entities;
using HospitalManagementSystem.Application.Common.Exceptions;
using HospitalManagementSystem.Domain.Interfaces;

namespace HospitalManagementSystem.Application.Services
{
    //public class PatientService : IPatientService
    //{
    //    private readonly IPatientRepository _patientRepository;
    //    private readonly IUnitOfWork _unitOfWork;

    //    public PatientService(IPatientRepository patientRepository, IUnitOfWork unitOfWork)
    //    {
    //        _patientRepository = patientRepository;
    //        _unitOfWork = unitOfWork;
    //    }

    //    public async Task<PatientDto> CreatePatientAsync(CreatePatientRequest request)
    //    {
    //        var patient = new Patient(
    //            request.FirstName,
    //            request.LastName,
    //            request.DateOfBirth,
    //            request.Gender,
    //            request.ContactNumber,
    //            request.Email,
    //            request.Address,
    //            request.EmergencyContact
    //        );

    //        if (!string.IsNullOrEmpty(request.MedicalHistory))
    //        {
    //            patient.UpdateMedicalHistory(request.MedicalHistory);
    //        }

    //        await _patientRepository.AddAsync(patient);
    //        await _unitOfWork.SaveChangesAsync();

    //        return new PatientDto
    //        {
    //            PatientId = patient.PatientId,
    //            FirstName = patient.FirstName,
    //            LastName = patient.LastName,
    //            DateOfBirth = patient.DateOfBirth,
    //            Gender = patient.Gender,
    //            BloodGroup = patient.BloodGroup,
    //            ContactNumber = patient.ContactNumber,
    //            Email = patient.Email,
    //            Address = patient.Address,
    //            EmergencyContact = patient.EmergencyContact,
    //            MedicalHistory = patient.MedicalHistory,
    //            Age = patient.GetAge(),
    //            IsActive = patient.IsActive,
    //            CreatedAt = patient.CreatedAt
    //        };
    //    }

    //    public async Task<PatientDto> GetPatientByIdAsync(int id)
    //    {
    //        var patient = await _patientRepository.GetByIdAsync(id);
    //        if (patient == null)
    //            throw new NotFoundException($"Patient with ID {id} not found");

    //        return new PatientDto
    //        {
    //            PatientId = patient.PatientId,
    //            FirstName = patient.FirstName,
    //            LastName = patient.LastName,
    //            DateOfBirth = patient.DateOfBirth,
    //            Gender = patient.Gender,
    //            BloodGroup = patient.BloodGroup,
    //            ContactNumber = patient.ContactNumber,
    //            Email = patient.Email,
    //            Address = patient.Address,
    //            EmergencyContact = patient.EmergencyContact,
    //            MedicalHistory = patient.MedicalHistory,
    //            Age = patient.GetAge(),
    //            IsActive = patient.IsActive,
    //            CreatedAt = patient.CreatedAt
    //        };
    //    }

    //    public async Task<IEnumerable<PatientDto>> GetAllPatientsAsync(bool includeInactive = false)
    //    {
    //        var patients = await _patientRepository.GetAllAsync(includeInactive);
    //        return patients.Select(p => new PatientDto
    //        {
    //            PatientId = p.PatientId,
    //            FirstName = p.FirstName,
    //            LastName = p.LastName,
    //            DateOfBirth = p.DateOfBirth,
    //            Gender = p.Gender,
    //            BloodGroup = p.BloodGroup,
    //            ContactNumber = p.ContactNumber,
    //            Email = p.Email,
    //            Address = p.Address,
    //            EmergencyContact = p.EmergencyContact,
    //            MedicalHistory = p.MedicalHistory,
    //            Age = p.GetAge(),
    //            IsActive = p.IsActive,
    //            CreatedAt = p.CreatedAt
    //        });
    //    }

    //    public async Task<PatientDto> UpdatePatientAsync(UpdatePatientRequest request)
    //    {
    //        var patient = await _patientRepository.GetByIdAsync(request.PatientId);
    //        if (patient == null)
    //            throw new NotFoundException($"Patient with ID {request.PatientId} not found");

    //        patient.UpdatePersonalInfo(
    //            request.FirstName,
    //            request.LastName,
    //            request.ContactNumber,
    //            request.Email,
    //            request.Address,
    //            request.EmergencyContact
    //        );

    //        if (!string.IsNullOrEmpty(request.MedicalHistory))
    //        {
    //            patient.UpdateMedicalHistory(request.MedicalHistory);
    //        }

    //        _patientRepository.Update(patient);
    //        await _unitOfWork.SaveChangesAsync();

    //        return new PatientDto
    //        {
    //            PatientId = patient.PatientId,
    //            FirstName = patient.FirstName,
    //            LastName = patient.LastName,
    //            DateOfBirth = patient.DateOfBirth,
    //            Gender = patient.Gender,
    //            BloodGroup = patient.BloodGroup,
    //            ContactNumber = patient.ContactNumber,
    //            Email = patient.Email,
    //            Address = patient.Address,
    //            EmergencyContact = patient.EmergencyContact,
    //            MedicalHistory = patient.MedicalHistory,
    //            Age = patient.GetAge(),
    //            IsActive = patient.IsActive,
    //            CreatedAt = patient.CreatedAt
    //        };
    //    }

    //    public async Task DeletePatientAsync(int id)
    //    {
    //        var patient = await _patientRepository.GetByIdAsync(id);
    //        if (patient == null)
    //            throw new NotFoundException($"Patient with ID {id} not found");

    //        patient.Deactivate();
    //        _patientRepository.Update(patient);
    //        await _unitOfWork.SaveChangesAsync();
    //    }
    //}
}
