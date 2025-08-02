using FluentValidation;

namespace HospitalManagementSystem.Application.Features.Rooms.Commands.Create;

public class CreateRoomCommandValidator : AbstractValidator<CreateRoomCommand>
{
    public CreateRoomCommandValidator()
    {
        RuleFor(x => x.RoomNumber).NotEmpty();
        RuleFor(x => x.RoomTypeId).GreaterThan(0);
        RuleFor(x => x.DepartmentId).GreaterThan(0);
        RuleFor(x => x.Capacity).GreaterThan(0);
    }
}

