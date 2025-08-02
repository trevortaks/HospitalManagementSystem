using System;
using System.Collections.Generic;

namespace HospitalManagementSystem.Application.DTOs;

public class PatientDto
{
    public int PatientId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string Gender { get; set; }
    public string BloodGroup { get; set; }
    public string ContactNumber { get; set; }
    public string Email { get; set; }
    public string Address { get; set; }
    public string EmergencyContact { get; set; }
    public string MedicalHistory { get; set; }
    public int Age { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public IEnumerable<MedicalRecordDto> MedicalRecords { get; set; }
    public IEnumerable<AppointmentDto> Appointments { get; set; }
    public IEnumerable<BillingDto> Bills { get; set; }
}