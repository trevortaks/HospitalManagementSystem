using HospitalManagementSystem.Application.Common.Models;

namespace HospitalManagementSystem.Application.Features.Doctors.Commands.Create;

public class CreateDoctorCommand : BaseCommand<int>
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Specialization { get; set; }
    public string LicenseNumber { get; set; }
    public string ContactNumber { get; set; }
    public string Email { get; set; }
    public int DepartmentId { get; set; }
}
