using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using HospitalManagementSystem.WebApp.Models;

namespace HospitalManagementSystem.WebApp.Services
{
    public class RoomService : IRoomService
    {
        private readonly HttpClient _httpClient;

        public RoomService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<RoomModel>> GetAllRoomsAsync()
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<RoomModel>>("api/rooms");
        }

        public async Task<RoomModel> GetRoomByIdAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<RoomModel>($"api/rooms/{id}");
        }

        public async Task<RoomModel> CreateRoomAsync(RoomModel room)
        {
            var response = await _httpClient.PostAsJsonAsync("api/rooms", room);
            return await response.Content.ReadFromJsonAsync<RoomModel>();
        }

        public async Task<RoomModel> UpdateRoomAsync(RoomModel room)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/rooms/{room.RoomId}", room);
            return await response.Content.ReadFromJsonAsync<RoomModel>();
        }

        public async Task DeleteRoomAsync(int id)
        {
            await _httpClient.DeleteAsync($"api/rooms/{id}");
        }
    }
}
