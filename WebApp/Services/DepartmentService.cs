using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using HospitalManagementSystem.WebApp.Models;

namespace HospitalManagementSystem.WebApp.Services
{
    public class DepartmentService : IDepartmentService
    {
        private readonly HttpClient _httpClient;

        public DepartmentService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<DepartmentModel>> GetAllDepartmentsAsync()
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<DepartmentModel>>("api/departments");
        }

        public async Task<DepartmentModel> GetDepartmentByIdAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<DepartmentModel>($"api/departments/{id}");
        }

        public async Task<DepartmentModel> CreateDepartmentAsync(DepartmentModel department)
        {
            var response = await _httpClient.PostAsJsonAsync("api/departments", department);
            return await response.Content.ReadFromJsonAsync<DepartmentModel>();
        }

        public async Task<DepartmentModel> UpdateDepartmentAsync(DepartmentModel department)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/departments/{department.DepartmentId}", department);
            return await response.Content.ReadFromJsonAsync<DepartmentModel>();
        }

        public async Task DeleteDepartmentAsync(int id)
        {
            await _httpClient.DeleteAsync($"api/departments/{id}");
        }
    }
}
