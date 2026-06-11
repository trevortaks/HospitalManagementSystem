using HospitalMS.Business.Models;

namespace HospitalMS.Business.Services;

public interface IAppointmentService
{
    Task<IReadOnlyList<AppointmentResponse>> GetAllAsync(Guid? patientId = null, Guid? doctorUserId = null, string? status = null, CancellationToken cancellationToken = default);
    Task<AppointmentResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<AppointmentResponse> CreateAsync(CreateAppointmentRequest request, CancellationToken cancellationToken = default);
    Task<AppointmentResponse> UpdateAsync(Guid id, UpdateAppointmentRequest request, CancellationToken cancellationToken = default);
    Task<AppointmentResponse> CancelAsync(Guid id, CancelAppointmentRequest request, CancellationToken cancellationToken = default);
    Task<AppointmentResponse> PatchStatusAsync(Guid id, PatchAppointmentStatusRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<AppointmentVitalsResponse> RecordVitalsAsync(Guid appointmentId, Guid recordedByUserId, RecordAppointmentVitalsRequest request, CancellationToken cancellationToken = default);
    Task<AppointmentVitalsResponse?> GetVitalsAsync(Guid appointmentId, CancellationToken cancellationToken = default);
}
