using HospitalManagementSystem.Application.Common.Models;

namespace HospitalManagementSystem.Application.Features.Appointments.Commands.Delete;

public class DeleteAppointmentCommand : BaseCommand
{
    public int AppointmentId { get; set; }
}

