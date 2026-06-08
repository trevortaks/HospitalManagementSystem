using HospitalMS.Business.Models;

namespace HospitalMS.Business.Services;

public interface IQualityService
{
    Task<QualityIncidentResponse> CreateIncidentAsync(CreateQualityIncidentRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<QualityIncidentResponse>> GetIncidentsAsync(string? status = null, string? severity = null, CancellationToken ct = default);
    Task<QualityIncidentResponse> GetIncidentByIdAsync(Guid id, CancellationToken ct = default);
    Task<QualityIncidentResponse> AssignIncidentAsync(Guid id, AssignQualityIncidentRequest request, CancellationToken ct = default);
    Task<QualityIncidentResponse> StartInvestigationAsync(Guid id, InvestigateQualityIncidentRequest request, CancellationToken ct = default);
    Task<QualityIncidentResponse> ResolveIncidentAsync(Guid id, ResolveQualityIncidentRequest request, CancellationToken ct = default);
    Task<QualityIncidentResponse> CloseIncidentAsync(Guid id, CancellationToken ct = default);

    Task<PatientFeedbackResponse> SubmitFeedbackAsync(CreatePatientFeedbackRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<PatientFeedbackResponse>> GetFeedbackAsync(Guid? patientId = null, CancellationToken ct = default);

    Task<QualitySummary> GetSummaryAsync(CancellationToken ct = default);
}
