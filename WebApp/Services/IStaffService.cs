using System.Collections.Generic;
using System.Threading.Tasks;
using HospitalManagementSystem.WebApp.Models;

namespace HospitalManagementSystem.WebApp.Services
{
    public interface IStaffService
    {
        Task<IEnumerable<StaffModel>> GetAllStaffAsync();
        Task<StaffModel> GetStaffByIdAsync(int id);
        Task<StaffModel> CreateStaffAsync(StaffModel staff);
        Task<StaffModel> UpdateStaffAsync(StaffModel staff);
        Task DeleteStaffAsync(int id);
    }
}
