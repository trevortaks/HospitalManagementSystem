using System;
using HospitalManagementSystem.Application.Common.Models;

namespace HospitalManagementSystem.Application.Features.Appointments.Commands.Create;

public class CreateAppointmentCommand : BaseCommand<int>
{
    public int PatientId { get; set; }
    public int DoctorId { get; set; }
    public DateTime AppointmentDate { get; set; }
    public DateTime EndTime { get; set; }
    public string Purpose { get; set; }
    public string Notes { get; set; }
    public int? RoomId { get; set; }
}