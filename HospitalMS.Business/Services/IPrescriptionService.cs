using HospitalMS.Business.Models;

namespace HospitalMS.Business.Services;

public interface IPrescriptionService
{
    Task<IReadOnlyList<PrescriptionResponse>> GetAllAsync(Guid? patientId = null, Guid? encounterId = null, string? status = null, CancellationToken cancellationToken = default);
    Task<PrescriptionResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PrescriptionResponse> CreateAsync(CreatePrescriptionRequest request, CancellationToken cancellationToken = default);
    Task<PrescriptionResponse> DispenseAsync(Guid id, DispensePrescriptionRequest request, CancellationToken cancellationToken = default);
    Task<PrescriptionResponse> CancelAsync(Guid id, CancellationToken cancellationToken = default);
}
