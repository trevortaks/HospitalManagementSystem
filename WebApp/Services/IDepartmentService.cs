using System.Collections.Generic;
using System.Threading.Tasks;
using HospitalManagementSystem.WebApp.Models;

namespace HospitalManagementSystem.WebApp.Services
{
    public interface IDepartmentService
    {
        Task<IEnumerable<DepartmentModel>> GetAllDepartmentsAsync();
        Task<DepartmentModel> GetDepartmentByIdAsync(int id);
        Task<DepartmentModel> CreateDepartmentAsync(DepartmentModel department);
        Task<DepartmentModel> UpdateDepartmentAsync(DepartmentModel department);
        Task DeleteDepartmentAsync(int id);
    }
}
