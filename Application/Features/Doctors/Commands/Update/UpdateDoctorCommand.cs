using HospitalManagementSystem.Application.Common.Models;

namespace HospitalManagementSystem.Application.Features.Doctors.Commands.Update;

public class UpdateDoctorCommand : BaseCommand
{
    public int DoctorId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Specialization { get; set; }
    public string ContactNumber { get; set; }
    public string Email { get; set; }
    public int DepartmentId { get; set; }
}
