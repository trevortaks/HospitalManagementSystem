using HospitalManagementSystem.Application.Common.Models;
using HospitalManagementSystem.Application.DTOs;

namespace HospitalManagementSystem.Application.Features.Departments.Queries.GetById;

public class GetDepartmentByIdQuery : BaseQuery<DepartmentDto>
{
    public int DepartmentId { get; set; }
}

