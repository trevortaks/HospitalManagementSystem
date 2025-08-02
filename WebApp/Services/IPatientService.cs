using System.Collections.Generic;
using System.Threading.Tasks;
using HospitalManagementSystem.WebApp.Models;

namespace HospitalManagementSystem.WebApp.Services
{
    public interface IPatientService
    {
        Task<IEnumerable<PatientModel>> GetAllPatientsAsync();
        Task<PatientModel> GetPatientByIdAsync(int id);
        Task<PatientModel> CreatePatientAsync(PatientModel patient);
        Task<PatientModel> UpdatePatientAsync(PatientModel patient);
        Task DeletePatientAsync(int id);
    }
}
