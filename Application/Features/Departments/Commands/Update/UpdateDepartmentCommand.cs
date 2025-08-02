using HospitalManagementSystem.Application.Common.Models;

namespace HospitalManagementSystem.Application.Features.Departments.Commands.Update;

public class UpdateDepartmentCommand : BaseCommand
{
    public int DepartmentId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Location { get; set; }
    public int? HeadDoctorId { get; set; }
}

