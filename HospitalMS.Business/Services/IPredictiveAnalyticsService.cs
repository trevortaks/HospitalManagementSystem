using HospitalMS.Business.Models;

namespace HospitalMS.Business.Services;

public interface IPredictiveAnalyticsService
{
    Task<IReadOnlyList<ReadmissionRiskScore>> GetReadmissionRisksAsync(int topN = 20, CancellationToken ct = default);
    Task<IReadOnlyList<BedDemandForecast>> GetBedDemandForecastAsync(int weeks = 4, CancellationToken ct = default);
    Task<IReadOnlyList<LowStockPrediction>> GetLowStockPredictionsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<AppointmentNoShowRisk>> GetNoShowRisksAsync(CancellationToken ct = default);
    Task<PredictiveInsightsSummary> GetInsightsSummaryAsync(CancellationToken ct = default);
}
