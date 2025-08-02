using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using HospitalManagementSystem.WebApp.Models;

namespace HospitalManagementSystem.WebApp.Services
{
    public class PatientService : IPatientService
    {
        private readonly HttpClient _httpClient;

        public PatientService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<PatientModel>> GetAllPatientsAsync()
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<PatientModel>>("api/patients");
        }

        public async Task<PatientModel> GetPatientByIdAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<PatientModel>($"api/patients/{id}");
        }

        public async Task<PatientModel> CreatePatientAsync(PatientModel patient)
        {
            var response = await _httpClient.PostAsJsonAsync("api/patients", patient);
            return await response.Content.ReadFromJsonAsync<PatientModel>();
        }

        public async Task<PatientModel> UpdatePatientAsync(PatientModel patient)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/patients/{patient.PatientId}", patient);
            return await response.Content.ReadFromJsonAsync<PatientModel>();
        }

        public async Task DeletePatientAsync(int id)
        {
            await _httpClient.DeleteAsync($"api/patients/{id}");
        }
    }
}
