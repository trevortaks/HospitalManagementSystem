using HospitalManagementSystem.Application.Common.Models;

namespace HospitalManagementSystem.Application.Features.Staff.Commands.Create;

public class CreateStaffCommand : BaseCommand<int>
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Position { get; set; }
    public string Specialization { get; set; }
    public string EmployeeId { get; set; }
    public int DepartmentId { get; set; }
    public string ContactNumber { get; set; }
    public string Email { get; set; }
    public string Address { get; set; }
    public DateTime DateOfBirth { get; set; }
    public DateTime HireDate { get; set; }
    public string Status { get; set; }
    public string EmploymentType { get; set; }
    public decimal Salary { get; set; }
    public string Shift { get; set; }
    public string WorkingHours { get; set; }
    public bool IsOnDuty { get; set; }
    public DateTime? LastLogin { get; set; }
    public string LicenseNumber { get; set; }
    public DateTime? LicenseExpiryDate { get; set; }
    public string Certification { get; set; }
    public DateTime? CertificationExpiryDate { get; set; }
    public string EmergencyContact { get; set; }
    public string EmergencyContactPhone { get; set; }
    public string BloodGroup { get; set; }
    public string Notes { get; set; }
}

