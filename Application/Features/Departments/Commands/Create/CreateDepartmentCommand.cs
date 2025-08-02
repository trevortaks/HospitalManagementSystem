using HospitalManagementSystem.Application.Common.Models;

namespace HospitalManagementSystem.Application.Features.Departments.Commands.Create;

public class CreateDepartmentCommand : BaseCommand<int>
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string Location { get; set; }
    public int? HeadDoctorId { get; set; }
}

