using HospitalMS.Business.Models;

namespace HospitalMS.Web.Models;

public sealed class DashboardViewModel
{
    public int TotalPatients { get; init; }
    public int TotalDoctors { get; init; }
    public int TotalNurses { get; init; }
    public int TotalStaff { get; init; }
    public IReadOnlyList<PatientResponse> RecentPatients { get; init; } = [];
    public IReadOnlyList<UserSummary> RecentStaff { get; init; } = [];
}
