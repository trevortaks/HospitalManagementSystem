using HospitalManagementSystem.Application.Common.Models;
using HospitalManagementSystem.Application.DTOs;

namespace HospitalManagementSystem.Application.Features.MedicalRecords.Queries.GetById;

public class GetMedicalRecordByIdQuery : BaseQuery<MedicalRecordDto>
{
    public int RecordId { get; set; }
}
