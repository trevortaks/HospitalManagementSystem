using System.Threading.Tasks;

namespace HospitalManagementSystem.Domain.Interfaces
{
    public interface IUnitOfWork
    {
        Task<int> SaveChangesAsync();
    }
}
