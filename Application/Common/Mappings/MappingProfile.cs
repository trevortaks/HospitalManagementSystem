using AutoMapper;
using HospitalManagementSystem.Application.DTOs;
using HospitalManagementSystem.Application.Features.Appointments.Commands.Create;
using HospitalManagementSystem.Application.Features.MedicalRecords.Commands;
using HospitalManagementSystem.Application.Features.Patients.Commands.Create;
using HospitalManagementSystem.Application.Features.Patients.Commands.Update;
using HospitalManagementSystem.Application.Features.Doctors.Commands.Create;
using HospitalManagementSystem.Application.Features.Doctors.Commands.Update;
using HospitalManagementSystem.Application.Features.Departments.Commands.Create;
using HospitalManagementSystem.Application.Features.Departments.Commands.Update;
using HospitalManagementSystem.Application.Features.Rooms.Commands.Create;
using HospitalManagementSystem.Application.Features.Rooms.Commands.Update;
using HospitalManagementSystem.Application.Features.Staff.Commands.Create;
using HospitalManagementSystem.Application.Features.Staff.Commands.Update;
using HospitalManagementSystem.Domain.Entities;

namespace HospitalManagementSystem.Application.Common.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Patient mappings
        CreateMap<Patient, PatientDto>()
            .ForMember(dest => dest.Age, opt => opt.MapFrom(src => CalculateAge(src.DateOfBirth)))
            .ForMember(dest => dest.MedicalRecords, opt => opt.MapFrom(src => src.MedicalRecords))
            .ForMember(dest => dest.Appointments, opt => opt.MapFrom(src => src.Appointments))
            .ForMember(dest => dest.Bills, opt => opt.MapFrom(src => src.Bills));

        CreateMap<CreatePatientCommand, Patient>();
        CreateMap<UpdatePatientCommand, Patient>();

        // Medical Record mappings
        CreateMap<MedicalRecord, MedicalRecordDto>();
        CreateMap<CreateMedicalRecordCommand, MedicalRecord>();

        // Appointment mappings
        CreateMap<Appointment, AppointmentDto>()
            .ForMember(dest => dest.PatientName, opt => opt.MapFrom(src => src.Patient.FirstName + " " + src.Patient.LastName))
            .ForMember(dest => dest.DoctorName, opt => opt.MapFrom(src => src.Doctor.FirstName + " " + src.Doctor.LastName))
            .ForMember(dest => dest.RoomNumber, opt => opt.MapFrom(src => src.Room != null ? src.Room.RoomNumber : null));

        CreateMap<CreateAppointmentCommand, Appointment>();

        // Doctor mappings
        CreateMap<Doctor, DoctorDto>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FirstName + " " + src.LastName));
        CreateMap<CreateDoctorCommand, Doctor>();
        CreateMap<UpdateDoctorCommand, Doctor>();

        // Additional entity mappings
        CreateMap<Department, DepartmentDto>();
        CreateMap<CreateDepartmentCommand, Department>();
        CreateMap<UpdateDepartmentCommand, Department>();

        CreateMap<Room, RoomDto>();
        CreateMap<CreateRoomCommand, Room>();
        CreateMap<UpdateRoomCommand, Room>();

        CreateMap<Staff, StaffDto>();
        CreateMap<CreateStaffCommand, Staff>();
        CreateMap<UpdateStaffCommand, Staff>();

        // Billing mappings
        CreateMap<Billing, BillingDto>();
        CreateMap<BillingItem, BillingItemDto>();
        CreateMap<Payment, PaymentDto>();
    }

    private int CalculateAge(DateTime dateOfBirth)
    {
        var today = DateTime.Today;
        var age = today.Year - dateOfBirth.Year;
        if (dateOfBirth.Date > today.AddYears(-age))
            age--;
        return age;
    }
}