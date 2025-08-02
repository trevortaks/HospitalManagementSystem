using HospitalManagementSystem.Application.Common.Models;
using HospitalManagementSystem.Application.DTOs;

namespace HospitalManagementSystem.Application.Features.Doctors.Queries.GetById;

public class GetDoctorByIdQuery : BaseQuery<DoctorDto>
{
    public int DoctorId { get; set; }
}
