using HospitalMS.Business.Models;
using HospitalMS.Common.Constants;
using HospitalMS.Data.Persistence.Entities;
using HospitalMS.Web.Filters;
using HospitalMS.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace HospitalMS.Web.Controllers;

[Route("dashboard")]
[RequireSession]
public sealed class DashboardController(IHttpClientFactory f) : AppController(f)
{

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var role   = HttpContext.Session.GetString("role") ?? string.Empty;
        var userId = HttpContext.Session.GetString("userId") ?? string.Empty;

        ViewData["Title"]      = "Dashboard";
        ViewData["ActivePage"] = "Dashboard";

        return role switch
        {
            UserRoles.Doctor  => await DoctorDashboard(userId, cancellationToken),
            UserRoles.Nurse   => await NurseDashboard(cancellationToken),
            UserRoles.Patient => await PatientDashboard(cancellationToken),
            _                 => await AdminDashboard(cancellationToken)
        };
    }

    // ── Admin / HR Manager / default ────────────────────────────────────────

    private async Task<IActionResult> AdminDashboard(CancellationToken ct)
    {
        var client = Api();

        var patientsTask = client.GetFromJsonAsync<IReadOnlyList<PatientResponse>>("/api/patients", ct);
        var usersTask    = client.GetFromJsonAsync<IReadOnlyList<UserSummary>>("/api/users", ct);
        var summaryTask  = client.GetFromJsonAsync<DashboardSummary>("/api/analytics/dashboard", ct);
        var hrTask       = client.GetFromJsonAsync<HRSummary>("/api/hr/summary", ct);

        await Task.WhenAll(patientsTask, usersTask, summaryTask, hrTask);

        var patients = await patientsTask ?? [];
        var staff    = await usersTask    ?? [];

        var vm = new DashboardViewModel
        {
            TotalPatients  = patients.Count,
            TotalDoctors   = staff.Count(u => u.Role == UserRoles.Doctor),
            TotalNurses    = staff.Count(u => u.Role == UserRoles.Nurse),
            TotalStaff     = staff.Count,
            RecentPatients = [.. patients.OrderByDescending(p => p.CreatedAtUtc).Take(6)],
            RecentStaff    = [.. staff.Take(8)],
            Summary        = await summaryTask,
            HRSummary      = await hrTask
        };

        return View("AdminIndex", vm);
    }

    // ── Doctor ──────────────────────────────────────────────────────────────

    private async Task<IActionResult> DoctorDashboard(string userId, CancellationToken ct)
    {
        if (!Guid.TryParse(userId, out var doctorId))
            return await AdminDashboard(ct);

        var client = Api();
        var today  = DateTime.UtcNow.Date;

        var staffTask      = client.GetFromJsonAsync<UserSummary>($"/api/users/{doctorId}", ct);
        var appointTask    = client.GetFromJsonAsync<IReadOnlyList<AppointmentResponse>>($"/api/appointments?doctorUserId={doctorId}", ct);
        var encountersTask = client.GetFromJsonAsync<IReadOnlyList<EncounterResponse>>($"/api/encounters?attendingDoctorId={doctorId}&isClosed=false", ct);

        await Task.WhenAll(staffTask, appointTask, encountersTask);

        var allAppts   = await appointTask    ?? [];
        var encounters = await encountersTask ?? [];

        var todayAppts = allAppts
            .Where(a => a.ScheduledAtUtc.Date == today
                     && a.Status != AppointmentStatus.Cancelled
                     && a.Status != AppointmentStatus.NoShow)
            .OrderBy(a => a.ScheduledAtUtc)
            .ToList();

        var completedToday = allAppts.Count(a =>
            a.ScheduledAtUtc.Date == today && a.Status == AppointmentStatus.Completed);

        var allEncounters = await client.GetFromJsonAsync<IReadOnlyList<EncounterResponse>>(
            $"/api/encounters?attendingDoctorId={doctorId}", ct) ?? [];

        var vm = new DoctorDashboardViewModel
        {
            Doctor            = await staffTask ?? new UserSummary(doctorId, userId, string.Empty, UserRoles.Doctor, true, DateTime.UtcNow),
            TodayAppointments = todayAppts,
            OpenEncounters    = encounters,
            TotalPatientsSeen = allEncounters.Select(e => e.PatientId).Distinct().Count(),
            CompletedToday    = completedToday
        };

        return View("DoctorIndex", vm);
    }

    // ── Patient ─────────────────────────────────────────────────────────────

    private async Task<IActionResult> PatientDashboard(CancellationToken ct)
    {
        var client   = Api();
        var response = await client.GetAsync("/api/portal/dashboard", ct);

        if (!response.IsSuccessStatusCode)
        {
            ViewData["NoLinkedPatient"] = true;
            return View("PatientIndex", (PatientDashboardViewModel?)null);
        }

        var dashboard = await response.Content.ReadFromJsonAsync<PortalDashboardResponse>(cancellationToken: ct);
        if (dashboard is null)
        {
            ViewData["NoLinkedPatient"] = true;
            return View("PatientIndex", (PatientDashboardViewModel?)null);
        }

        var next = dashboard.UpcomingAppointments
            .Where(a => a.ScheduledAtUtc >= DateTime.UtcNow
                     && a.Status != "Cancelled" && a.Status != "NoShow")
            .OrderBy(a => a.ScheduledAtUtc)
            .FirstOrDefault();

        await TryLogPortalSessionAsync(client, dashboard.Profile.PatientId, ct);

        return View("PatientIndex", new PatientDashboardViewModel
        {
            Dashboard       = dashboard,
            NextAppointment = next
        });
    }

    private async Task TryLogPortalSessionAsync(HttpClient client, Guid patientId, CancellationToken ct)
    {
        try
        {
            await client.PostAsJsonAsync("/api/portal/session",
                new { UserId = Guid.Empty, PatientId = patientId, IpAddress = (string?)null, UserAgent = (string?)null }, ct);
        }
        catch { }
    }

    // ── Nurse ────────────────────────────────────────────────────────────────

    private async Task<IActionResult> NurseDashboard(CancellationToken ct)
    {
        var client = Api();
        var today  = DateTime.UtcNow.Date;

        var appointments = await client.GetFromJsonAsync<IReadOnlyList<AppointmentResponse>>(
            "/api/appointments", ct) ?? [];

        var queue = appointments
            .Where(a => a.ScheduledAtUtc.Date == today
                     && a.Status != AppointmentStatus.Cancelled
                     && a.Status != AppointmentStatus.NoShow
                     && a.Status != AppointmentStatus.Completed)
            .OrderBy(a => a.ScheduledAtUtc)
            .ToList();

        var vitalsRecorded = appointments.Count(a =>
            a.ScheduledAtUtc.Date == today && a.PreConsultVitals is not null);

        var vm = new NurseDashboardViewModel
        {
            TriageQueue        = queue,
            VitalsRecordedToday = vitalsRecorded,
            PendingTriage      = queue.Count(a => a.PreConsultVitals is null)
        };

        return View("NurseIndex", vm);
    }

}
