using HospitalMS.Business.Models;

namespace HospitalMS.Business.Services;

public interface IPortalService
{
    Task<PortalProfileResponse?> GetProfileAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<PortalProfileResponse?> UpdateProfileAsync(Guid userId, UpdatePortalProfileRequest request, CancellationToken cancellationToken = default);
    Task<PortalDashboardResponse?> GetDashboardAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PortalAppointmentResponse>> GetAppointmentsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PortalEncounterResponse>> GetEncountersAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PortalPrescriptionResponse>> GetPrescriptionsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task LogSessionAsync(LogPortalSessionRequest request, CancellationToken cancellationToken = default);
}
