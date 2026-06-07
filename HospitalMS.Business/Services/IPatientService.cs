using HospitalMS.Business.Models;

namespace HospitalMS.Business.Services;

public interface IPatientService
{
    Task<IReadOnlyList<PatientResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<PatientResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PatientResponse> CreateAsync(CreatePatientRequest request, CancellationToken cancellationToken = default);
    Task<PatientResponse> UpdateAsync(Guid id, UpdatePatientRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
