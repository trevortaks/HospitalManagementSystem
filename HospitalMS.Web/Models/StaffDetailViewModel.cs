using HospitalMS.Business.Models;

namespace HospitalMS.Web.Models;

public sealed class StaffDetailViewModel
{
    public UserSummary Staff { get; init; } = null!;
    public EmployeeRecordResponse? EmployeeRecord { get; init; }
    public IReadOnlyList<AppointmentResponse> Appointments { get; init; } = [];
    public IReadOnlyList<EncounterResponse> Encounters { get; init; } = [];
    public IReadOnlyList<LeaveRequestResponse> LeaveRequests { get; init; } = [];
}
