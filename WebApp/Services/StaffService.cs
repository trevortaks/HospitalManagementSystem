using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using HospitalManagementSystem.WebApp.Models;

namespace HospitalManagementSystem.WebApp.Services
{
    public class StaffService : IStaffService
    {
        private readonly HttpClient _httpClient;

        public StaffService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<StaffModel>> GetAllStaffAsync()
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<StaffModel>>("api/staff");
        }

        public async Task<StaffModel> GetStaffByIdAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<StaffModel>($"api/staff/{id}");
        }

        public async Task<StaffModel> CreateStaffAsync(StaffModel staff)
        {
            var response = await _httpClient.PostAsJsonAsync("api/staff", staff);
            return await response.Content.ReadFromJsonAsync<StaffModel>();
        }

        public async Task<StaffModel> UpdateStaffAsync(StaffModel staff)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/staff/{staff.StaffId}", staff);
            return await response.Content.ReadFromJsonAsync<StaffModel>();
        }

        public async Task DeleteStaffAsync(int id)
        {
            await _httpClient.DeleteAsync($"api/staff/{id}");
        }
    }
}
