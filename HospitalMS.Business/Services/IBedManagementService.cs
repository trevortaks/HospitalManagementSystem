using HospitalMS.Business.Models;

namespace HospitalMS.Business.Services;

public interface IBedManagementService
{
    // Wards
    Task<IReadOnlyList<WardResponse>> GetAllWardsAsync(bool? activeOnly = null, CancellationToken cancellationToken = default);
    Task<WardResponse?> GetWardByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<WardResponse> CreateWardAsync(CreateWardRequest request, CancellationToken cancellationToken = default);
    Task<WardResponse> UpdateWardAsync(Guid id, UpdateWardRequest request, CancellationToken cancellationToken = default);
    Task<WardResponse> ToggleWardActiveAsync(Guid id, CancellationToken cancellationToken = default);

    // Beds
    Task<IReadOnlyList<BedResponse>> GetBedsByWardAsync(Guid wardId, CancellationToken cancellationToken = default);
    Task<BedResponse?> GetBedByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<BedResponse> CreateBedAsync(CreateBedRequest request, CancellationToken cancellationToken = default);
    Task<BedResponse> UpdateBedStatusAsync(Guid id, string status, CancellationToken cancellationToken = default);
    Task<BedResponse> ToggleBedActiveAsync(Guid id, CancellationToken cancellationToken = default);

    // Allocations
    Task<IReadOnlyList<BedAllocationResponse>> GetActiveAllocationsAsync(Guid? wardId = null, CancellationToken cancellationToken = default);
    Task<BedAllocationResponse?> GetPatientCurrentAllocationAsync(Guid patientId, CancellationToken cancellationToken = default);
    Task<BedAllocationResponse> AllocateBedAsync(AllocateBedRequest request, CancellationToken cancellationToken = default);
    Task<BedAllocationResponse> DischargePatientAsync(DischargePatientRequest request, CancellationToken cancellationToken = default);

    // Dashboard
    Task<BedAvailabilityResponse> GetBedAvailabilityAsync(CancellationToken cancellationToken = default);
}
