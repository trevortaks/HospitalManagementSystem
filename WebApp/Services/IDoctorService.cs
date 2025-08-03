using System.Collections.Generic;
using System.Threading.Tasks;
using HospitalManagementSystem.WebApp.Models;

namespace HospitalManagementSystem.WebApp.Services
{
    public interface IDoctorService
    {
        Task<IEnumerable<DoctorModel>> GetAllDoctorsAsync();
        Task<DoctorModel> GetDoctorByIdAsync(int id);
        Task<DoctorModel> CreateDoctorAsync(DoctorModel doctor);
        Task<DoctorModel> UpdateDoctorAsync(DoctorModel doctor);
        Task DeleteDoctorAsync(int id);
    }
}
