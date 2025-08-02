using HospitalManagementSystem.Application.Common.Models;

namespace HospitalManagementSystem.Application.Features.Rooms.Commands.Delete;

public class DeleteRoomCommand : BaseCommand
{
    public int RoomId { get; set; }
}

