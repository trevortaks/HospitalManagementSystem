namespace HospitalManagementSystem.Application.Features.MedicalRecords.Commands;

using FluentValidation;

public class CreateMedicalRecordCommandValidator : AbstractValidator<CreateMedicalRecordCommand>
{
    public CreateMedicalRecordCommandValidator()
    {
        RuleFor(x => x.PatientId)
            .GreaterThan(0).WithMessage("Patient ID must be greater than 0.");

        RuleFor(x => x.DoctorId)
            .GreaterThan(0).WithMessage("Doctor ID must be greater than 0.");

        RuleFor(x => x.VisitDate)
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Visit date cannot be in the future.");

        RuleFor(x => x.Diagnosis)
            .NotEmpty().WithMessage("Diagnosis is required.")
            .MaximumLength(500);

        RuleFor(x => x.Treatment)
            .MaximumLength(1000);

        RuleFor(x => x.Notes)
            .MaximumLength(2000);
    }
}
