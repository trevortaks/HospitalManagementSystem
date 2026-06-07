using HospitalMS.Business.Models;

namespace HospitalMS.Business.Services;

public interface IMedicationService
{
    Task<IReadOnlyList<MedicationResponse>> GetAllAsync(bool? activeOnly = null, CancellationToken cancellationToken = default);
    Task<MedicationResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<MedicationResponse> CreateAsync(CreateMedicationRequest request, CancellationToken cancellationToken = default);
    Task<MedicationResponse> UpdateAsync(Guid id, UpdateMedicationRequest request, CancellationToken cancellationToken = default);
    Task<MedicationResponse> ToggleActiveAsync(Guid id, CancellationToken cancellationToken = default);
}
