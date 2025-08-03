using System.Collections.Generic;
using System.Threading.Tasks;
using HospitalManagementSystem.WebApp.Models;

namespace HospitalManagementSystem.WebApp.Services
{
    public interface IRoomService
    {
        Task<IEnumerable<RoomModel>> GetAllRoomsAsync();
        Task<RoomModel> GetRoomByIdAsync(int id);
        Task<RoomModel> CreateRoomAsync(RoomModel room);
        Task<RoomModel> UpdateRoomAsync(RoomModel room);
        Task DeleteRoomAsync(int id);
    }
}
