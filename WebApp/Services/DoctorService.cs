using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using HospitalManagementSystem.WebApp.Models;

namespace HospitalManagementSystem.WebApp.Services
{
    public class DoctorService : IDoctorService
    {
        private readonly HttpClient _httpClient;

        public DoctorService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<DoctorModel>> GetAllDoctorsAsync()
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<DoctorModel>>("api/doctors");
        }

        public async Task<DoctorModel> GetDoctorByIdAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<DoctorModel>($"api/doctors/{id}");
        }

        public async Task<DoctorModel> CreateDoctorAsync(DoctorModel doctor)
        {
            var response = await _httpClient.PostAsJsonAsync("api/doctors", doctor);
            return await response.Content.ReadFromJsonAsync<DoctorModel>();
        }

        public async Task<DoctorModel> UpdateDoctorAsync(DoctorModel doctor)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/doctors/{doctor.DoctorId}", doctor);
            return await response.Content.ReadFromJsonAsync<DoctorModel>();
        }

        public async Task DeleteDoctorAsync(int id)
        {
            await _httpClient.DeleteAsync($"api/doctors/{id}");
        }
    }
}
