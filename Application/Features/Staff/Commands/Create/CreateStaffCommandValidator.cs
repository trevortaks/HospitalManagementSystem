using FluentValidation;

namespace HospitalManagementSystem.Application.Features.Staff.Commands.Create;

public class CreateStaffCommandValidator : AbstractValidator<CreateStaffCommand>
{
    public CreateStaffCommandValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty();
        RuleFor(x => x.LastName).NotEmpty();
        RuleFor(x => x.Position).NotEmpty();
        RuleFor(x => x.EmployeeId).NotEmpty();
        RuleFor(x => x.DepartmentId).GreaterThan(0);
        RuleFor(x => x.ContactNumber).NotEmpty();
        RuleFor(x => x.Address).NotEmpty();
        RuleFor(x => x.DateOfBirth).NotEmpty();
        RuleFor(x => x.HireDate).NotEmpty();
        RuleFor(x => x.EmploymentType).NotEmpty();
        RuleFor(x => x.Salary).GreaterThan(0);
        RuleFor(x => x.Shift).NotEmpty();
        RuleFor(x => x.WorkingHours).NotEmpty();
    }
}

