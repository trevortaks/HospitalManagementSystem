using HospitalManagementSystem.Application.Common.Models;

namespace HospitalManagementSystem.Application.Features.Appointments.Commands.Update;

public class UpdateAppointmentCommand : BaseCommand
{
    public int AppointmentId { get; set; }
    public int PatientId { get; set; }
    public int DoctorId { get; set; }
    public DateTime AppointmentDate { get; set; }
    public DateTime EndTime { get; set; }
    public string Status { get; set; }
    public string Purpose { get; set; }
    public string Notes { get; set; }
    public int? RoomId { get; set; }
}

