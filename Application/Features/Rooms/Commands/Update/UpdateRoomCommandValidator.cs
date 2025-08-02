using FluentValidation;

namespace HospitalManagementSystem.Application.Features.Rooms.Commands.Update;

public class UpdateRoomCommandValidator : AbstractValidator<UpdateRoomCommand>
{
    public UpdateRoomCommandValidator()
    {
        RuleFor(x => x.RoomId)
            .GreaterThan(0);

        RuleFor(x => x.RoomNumber)
            .NotEmpty();

        RuleFor(x => x.RoomTypeId)
            .GreaterThan(0);

        RuleFor(x => x.DepartmentId)
            .GreaterThan(0);

        RuleFor(x => x.Capacity)
            .GreaterThan(0);

        RuleFor(x => x.CurrentOccupancy)
            .GreaterThanOrEqualTo(0);
    }
}

