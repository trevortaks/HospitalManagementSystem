using HospitalManagementSystem.Application.Common.Models;
using HospitalManagementSystem.Application.DTOs;

namespace HospitalManagementSystem.Application.Features.Appointments.Queries.GetById;

public class GetAppointmentByIdQuery : BaseQuery<AppointmentDto>
{
    public int AppointmentId { get; set; }
}
