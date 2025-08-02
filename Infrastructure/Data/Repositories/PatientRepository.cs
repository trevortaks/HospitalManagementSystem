using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using HospitalManagementSystem.Domain.Entities;
using HospitalManagementSystem.Domain.Interfaces;

namespace HospitalManagementSystem.Infrastructure.Data.Repositories
{
    //public class PatientRepository : Repository<Patient>, IPatientRepository
    //{
    //    public PatientRepository(ApplicationDbContext context) : base(context) { }

    //    public async Task<IEnumerable<Patient>> GetAllAsync(bool includeInactive = false)
    //    {
    //        if (includeInactive)
    //        {
    //            return await _context.Patients.ToListAsync();
    //        }
    //        return await _context.Patients.Where(p => p.IsActive).ToListAsync();
    //    }

    //    public async Task<IEnumerable<Patient>> GetPatientsByDoctorAsync(int doctorId)
    //    {
    //        return await _context.Patients
    //            .Where(p => p.IsActive && 
    //                       (p.MedicalRecords.Any(mr => mr.DoctorId == doctorId) ||
    //                        p.Appointments.Any(a => a.DoctorId == doctorId)))
    //            .ToListAsync();
    //    }
    //}
}
