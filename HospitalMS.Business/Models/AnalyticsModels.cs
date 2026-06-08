namespace HospitalMS.Business.Models;

public record DashboardSummary(
    int TotalPatients,
    int NewPatientsThisMonth,
    int AppointmentsToday,
    int AppointmentsThisWeek,
    int OccupiedBeds,
    int TotalBeds,
    decimal OccupancyRate,
    int OpenMaintenanceRequests,
    decimal OutstandingInvoiceBalance,
    int LowStockItemCount);

public record MonthlyCount(int Year, int Month, string MonthLabel, int Count);
public record MonthlyRevenue(int Year, int Month, string MonthLabel, decimal Revenue);
public record LabelCount(string Label, int Count);
public record LabelAmount(string Label, decimal Amount);

public record PatientDemographicsReport(
    int TotalPatients,
    IReadOnlyList<LabelCount> ByGender,
    IReadOnlyList<LabelCount> ByAgeGroup,
    IReadOnlyList<MonthlyCount> NewPatientsPerMonth);

public record AppointmentReport(
    int TotalAppointments,
    IReadOnlyList<LabelCount> ByStatus,
    IReadOnlyList<MonthlyCount> PerMonth);

public record RevenueReport(
    decimal TotalRevenue,
    decimal OutstandingBalance,
    IReadOnlyList<MonthlyRevenue> RevenuePerMonth,
    IReadOnlyList<LabelAmount> ByPaymentMethod);

public record WardOccupancyRow(
    Guid WardId,
    string WardName,
    int TotalBeds,
    int OccupiedBeds,
    decimal OccupancyRate);

public record BedOccupancyReport(
    int TotalBeds,
    int OccupiedBeds,
    int AvailableBeds,
    decimal OccupancyRate,
    IReadOnlyList<WardOccupancyRow> ByWard);

public record LowStockItemSummary(
    Guid Id,
    string Code,
    string Name,
    string Unit,
    int CurrentStock,
    int ReorderLevel);

public record InventoryStatusReport(
    int TotalActiveItems,
    int LowStockCount,
    IReadOnlyList<LowStockItemSummary> LowStockItems);
