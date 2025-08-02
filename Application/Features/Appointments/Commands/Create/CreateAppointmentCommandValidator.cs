using FluentValidation;
using HospitalManagementSystem.Application.Features.Appointments.Commands.Create;

namespace HospitalManagementSystem.Application.Features.Appointments.Commands.Create;

public class CreateAppointmentCommandValidator : AbstractValidator<CreateAppointmentCommand>
{
    public CreateAppointmentCommandValidator()
    {
        RuleFor(v => v.PatientId)
            .NotEmpty().WithMessage("Patient ID is required.");

        RuleFor(v => v.DoctorId)
            .NotEmpty().WithMessage("Doctor ID is required.");

        RuleFor(v => v.AppointmentDate)
            .NotEmpty().WithMessage("Appointment date is required.")
            .GreaterThan(DateTime.Now).WithMessage("Appointment date must be in the future.");

        RuleFor(v => v.EndTime)
            .NotEmpty().WithMessage("End time is required.")
            .GreaterThan(v => v.AppointmentDate).WithMessage("End time must be after appointment date.");

        RuleFor(v => v.Purpose)
            .NotEmpty().WithMessage("Purpose is required.")
            .MaximumLength(500).WithMessage("Purpose must not exceed 500 characters.");

        RuleFor(v => v)
            .MustAsync(HaveNoOverlappingAppointments).WithMessage("Doctor has overlapping appointment at this time.");
    }

    private async Task<bool> HaveNoOverlappingAppointments(CreateAppointmentCommand command, CancellationToken cancellationToken)
    {
        // This would need to be implemented with dependency injection
        // For now, return true as placeholder
        return await Task.FromResult(true);
    }
}