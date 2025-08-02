using FluentValidation;

namespace HospitalManagementSystem.Application.Features.Patients.Commands.Update;

public class UpdatePatientCommandValidator : AbstractValidator<UpdatePatientCommand>
{
    public UpdatePatientCommandValidator()
    {
        RuleFor(v => v.PatientId)
            .GreaterThan(0);

        RuleFor(v => v.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(50).WithMessage("First name must not exceed 50 characters.");

        RuleFor(v => v.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(50).WithMessage("Last name must not exceed 50 characters.");

        RuleFor(v => v.ContactNumber)
            .NotEmpty().WithMessage("Contact number is required.")
            .Matches(@"^\+?[\d\s\-\(\)]+$").WithMessage("Invalid contact number format.");

        RuleFor(v => v.Email)
            .EmailAddress().WithMessage("Invalid email format.")
            .When(v => !string.IsNullOrEmpty(v.Email));

        RuleFor(v => v.Address)
            .NotEmpty().WithMessage("Address is required.")
            .MaximumLength(200).WithMessage("Address must not exceed 200 characters.");

        RuleFor(v => v.EmergencyContact)
            .NotEmpty().WithMessage("Emergency contact is required.")
            .MaximumLength(100).WithMessage("Emergency contact must not exceed 100 characters.");
    }
}

