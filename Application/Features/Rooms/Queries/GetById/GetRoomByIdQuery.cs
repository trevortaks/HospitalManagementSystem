using HospitalManagementSystem.Application.Common.Models;
using HospitalManagementSystem.Application.DTOs;

namespace HospitalManagementSystem.Application.Features.Rooms.Queries.GetById;

public class GetRoomByIdQuery : BaseQuery<RoomDto>
{
    public int RoomId { get; set; }
}

