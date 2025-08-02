using HospitalManagementSystem.Application.Common.Models;

namespace HospitalManagementSystem.Application.Features.Rooms.Commands.Create;

public class CreateRoomCommand : BaseCommand<int>
{
    public string RoomNumber { get; set; }
    public int RoomTypeId { get; set; }
    public string Status { get; set; }
    public int Capacity { get; set; }
    public int CurrentOccupancy { get; set; }
    public int FloorNumber { get; set; }
    public int DepartmentId { get; set; }
}

