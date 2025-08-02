using FluentValidation;

namespace HospitalManagementSystem.Application.Features.Doctors.Commands.Update;

public class UpdateDoctorCommandValidator : AbstractValidator<UpdateDoctorCommand>
{
    public UpdateDoctorCommandValidator()
    {
        RuleFor(x => x.DoctorId)
            .GreaterThan(0);

        RuleFor(x => x.FirstName)
            .NotEmpty();

        RuleFor(x => x.LastName)
            .NotEmpty();

        RuleFor(x => x.Specialization)
            .NotEmpty();

        RuleFor(x => x.ContactNumber)
            .NotEmpty();

        RuleFor(x => x.DepartmentId)
            .GreaterThan(0);

        RuleFor(x => x.Email)
            .EmailAddress()
            .When(x => !string.IsNullOrEmpty(x.Email));
    }
}

