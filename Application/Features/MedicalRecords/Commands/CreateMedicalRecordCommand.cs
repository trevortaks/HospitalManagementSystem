using MediatR;

namespace HospitalManagementSystem.Application.Features.MedicalRecords.Commands;

public class CreateMedicalRecordCommand : IRequest<int> // Assuming MediatR
{
    public int PatientId { get; set; }
    public int DoctorId { get; set; }
    public DateTime VisitDate { get; set; }
    public string Diagnosis { get; set; }
    public string Treatment { get; set; }
    public string Notes { get; set; }
}
