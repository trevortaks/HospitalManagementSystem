namespace HospitalMS.Business.Models;

public record ReadmissionRiskScore(
    Guid PatientId,
    string PatientName,
    double RiskScore,
    string RiskLevel,
    IReadOnlyList<string> RiskFactors);

public record BedDemandForecast(
    string Period,
    int PredictedAdmissions,
    int RecommendedAvailableBeds,
    double ConfidenceLevel);

public record LowStockPrediction(
    Guid ItemId,
    string ItemCode,
    string ItemName,
    string Unit,
    int CurrentStock,
    int ReorderLevel,
    double AverageDailyUsage,
    int EstimatedDaysUntilStockout,
    string Urgency);

public record AppointmentNoShowRisk(
    Guid AppointmentId,
    Guid PatientId,
    string PatientName,
    DateTime ScheduledAtUtc,
    double NoShowProbability,
    string RiskLevel);

public record PredictiveInsightsSummary(
    int HighRiskReadmissions,
    int CriticalStockItems,
    int HighNoShowRiskAppointments,
    IReadOnlyList<ReadmissionRiskScore> TopReadmissionRisks,
    IReadOnlyList<LowStockPrediction> CriticalStockPredictions,
    IReadOnlyList<BedDemandForecast> WeeklyBedForecast);
