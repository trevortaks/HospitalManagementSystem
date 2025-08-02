using System.Collections.Generic;
using HospitalManagementSystem.Application.Common.Models;
using HospitalManagementSystem.Application.DTOs;

namespace HospitalManagementSystem.Application.Features.Patients.Queries.GetAll;

public class GetAllPatientsQuery : BaseQuery<IEnumerable<PatientDto>>
{
    public bool IncludeInactive { get; set; } = false;
    public string SearchTerm { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}