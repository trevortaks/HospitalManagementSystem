using HospitalMS.Business.Models;

namespace HospitalMS.Business.Services;

public interface IAnalyticsService
{
    Task<DashboardSummary> GetDashboardSummaryAsync(CancellationToken cancellationToken = default);
    Task<PatientDemographicsReport> GetPatientDemographicsAsync(CancellationToken cancellationToken = default);
    Task<AppointmentReport> GetAppointmentReportAsync(DateTime? from = null, DateTime? to = null, CancellationToken cancellationToken = default);
    Task<RevenueReport> GetRevenueReportAsync(CancellationToken cancellationToken = default);
    Task<BedOccupancyReport> GetBedOccupancyReportAsync(CancellationToken cancellationToken = default);
    Task<InventoryStatusReport> GetInventoryStatusReportAsync(CancellationToken cancellationToken = default);
}
