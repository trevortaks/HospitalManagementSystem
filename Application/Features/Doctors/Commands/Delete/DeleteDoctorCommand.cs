using HospitalManagementSystem.Application.Common.Models;

namespace HospitalManagementSystem.Application.Features.Doctors.Commands.Delete;

public class DeleteDoctorCommand : BaseCommand
{
    public int DoctorId { get; set; }
}
