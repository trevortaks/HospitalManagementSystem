using HospitalManagementSystem.Application.Common.Models;
using HospitalManagementSystem.Application.DTOs;

namespace HospitalManagementSystem.Application.Features.Patients.Queries.GetById;

public class GetPatientByIdQuery : BaseQuery<PatientDto>
{
    public int Id { get; set; }
}