using HospitalManagementSystem.Application.Common.Models;

namespace HospitalManagementSystem.Application.Features.Staff.Commands.Delete;

public class DeleteStaffCommand : BaseCommand
{
    public int StaffId { get; set; }
}

