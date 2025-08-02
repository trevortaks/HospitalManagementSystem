using HospitalManagementSystem.Application.Common.Models;

namespace HospitalManagementSystem.Application.Features.Patients.Commands.Delete;

public class DeletePatientCommand : BaseCommand
{
    public int PatientId { get; set; }
}