using FluentValidation;

namespace HospitalManagementSystem.Application.Features.Doctors.Commands.Create;

public class CreateDoctorCommandValidator : AbstractValidator<CreateDoctorCommand>
{
    public CreateDoctorCommandValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty();
        RuleFor(x => x.LastName).NotEmpty();
        RuleFor(x => x.Specialization).NotEmpty();
        RuleFor(x => x.LicenseNumber).NotEmpty();
        RuleFor(x => x.DepartmentId).GreaterThan(0);
    }
}
