using System;

namespace HospitalManagementSystem.Application.DTOs;

public class AppointmentDto
{
    public int AppointmentId { get; set; }
    public int PatientId { get; set; }
    public string PatientName { get; set; }
    public int DoctorId { get; set; }
    public string DoctorName { get; set; }
    public DateTime AppointmentDate { get; set; }
    public DateTime EndTime { get; set; }
    public string Status { get; set; }
    public string Purpose { get; set; }
    public string Notes { get; set; }
    public int? RoomId { get; set; }
    public string RoomNumber { get; set; }
    public DateTime CreatedAt { get; set; }
}