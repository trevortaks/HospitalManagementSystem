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
    public DashboardSummary? Summary { get; init; }
    public HRSummary? HRSummary { get; init; }
}

public sealed class DoctorDashboardViewModel
{
    public UserSummary Doctor { get; init; } = null!;
    public IReadOnlyList<AppointmentResponse> TodayAppointments { get; init; } = [];
    public IReadOnlyList<EncounterResponse> OpenEncounters { get; init; } = [];
    public int TotalPatientsSeen { get; init; }
    public int CompletedToday { get; init; }
}

public sealed class NurseDashboardViewModel
{
    public IReadOnlyList<AppointmentResponse> TriageQueue { get; init; } = [];
    public int VitalsRecordedToday { get; init; }
    public int PendingTriage { get; init; }
}
