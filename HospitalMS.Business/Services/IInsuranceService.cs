using HospitalMS.Business.Models;

namespace HospitalMS.Business.Services;

public interface IInsuranceService
{
    Task<IReadOnlyList<InsuranceProviderResponse>> GetAllProvidersAsync(bool? activeOnly = null, CancellationToken cancellationToken = default);
    Task<InsuranceProviderResponse?> GetProviderByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<InsuranceProviderResponse> CreateProviderAsync(CreateInsuranceProviderRequest request, CancellationToken cancellationToken = default);
    Task<InsuranceProviderResponse> ToggleProviderActiveAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PatientInsuranceResponse>> GetPatientInsuranceAsync(Guid patientId, CancellationToken cancellationToken = default);
    Task<PatientInsuranceResponse> AddPatientInsuranceAsync(AddPatientInsuranceRequest request, CancellationToken cancellationToken = default);
    Task<PatientInsuranceResponse> SetPrimaryAsync(Guid patientInsuranceId, CancellationToken cancellationToken = default);
    Task DeletePatientInsuranceAsync(Guid patientInsuranceId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<InsuranceClaimResponse>> GetClaimsForInvoiceAsync(Guid invoiceId, CancellationToken cancellationToken = default);
    Task<InsuranceClaimResponse> CreateClaimAsync(CreateInsuranceClaimRequest request, CancellationToken cancellationToken = default);
    Task<InsuranceClaimResponse> UpdateClaimStatusAsync(Guid claimId, UpdateClaimStatusRequest request, CancellationToken cancellationToken = default);
}
