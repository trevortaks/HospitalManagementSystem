using HospitalManagementSystem.Application.Common.Models;
using HospitalManagementSystem.Application.DTOs;

namespace HospitalManagementSystem.Application.Features.Staff.Queries.GetById;

public class GetStaffByIdQuery : BaseQuery<StaffDto>
{
    public int StaffId { get; set; }
}

