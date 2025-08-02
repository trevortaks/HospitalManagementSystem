using HospitalManagementSystem.Application.Common.Models;

namespace HospitalManagementSystem.Application.Features.Departments.Commands.Delete;

public class DeleteDepartmentCommand : BaseCommand
{
    public int DepartmentId { get; set; }
}

