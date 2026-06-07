using HospitalMS.Business.Models;

namespace HospitalMS.Business.Services;

public interface IEncounterService
{
    Task<IReadOnlyList<EncounterResponse>> GetAllAsync(Guid? patientId = null, bool? isClosed = null, CancellationToken cancellationToken = default);
    Task<EncounterResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<EncounterResponse> CreateAsync(CreateEncounterRequest request, CancellationToken cancellationToken = default);
    Task<EncounterResponse> UpdateAsync(Guid id, UpdateEncounterRequest request, CancellationToken cancellationToken = default);
    Task<EncounterResponse> CloseAsync(Guid id, CancellationToken cancellationToken = default);
    Task<DiagnosisResponse> AddDiagnosisAsync(Guid encounterId, AddDiagnosisRequest request, CancellationToken cancellationToken = default);
    Task DeleteDiagnosisAsync(Guid encounterId, Guid diagnosisId, CancellationToken cancellationToken = default);
    Task<VitalsResponse> AddVitalsAsync(Guid encounterId, Guid recordedByUserId, AddVitalsRequest request, CancellationToken cancellationToken = default);
}
