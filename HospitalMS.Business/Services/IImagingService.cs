using HospitalMS.Business.Models;

namespace HospitalMS.Business.Services;

public interface IImagingService
{
    Task<IReadOnlyList<ImagingRequestResponse>> GetAllRequestsAsync(Guid? patientId = null, Guid? encounterId = null, string? status = null, CancellationToken cancellationToken = default);
    Task<ImagingRequestResponse?> GetRequestByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ImagingRequestResponse> CreateRequestAsync(CreateImagingRequestRequest request, CancellationToken cancellationToken = default);
    Task<ImagingRequestResponse> UpdateStatusAsync(Guid id, string status, CancellationToken cancellationToken = default);
    Task<ImagingRequestResponse> CancelRequestAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ImagingRequestResponse> CreateReportAsync(Guid requestId, CreateImagingReportRequest request, CancellationToken cancellationToken = default);
}
