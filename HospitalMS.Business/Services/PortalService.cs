using HospitalMS.Business.Models;
using HospitalMS.Data.Persistence;
using HospitalMS.Data.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace HospitalMS.Business.Services;

public sealed class PortalService(HospitalDbContext dbContext) : IPortalService
{
    public async Task<PortalProfileResponse?> GetProfileAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await dbContext.Users
            .AsNoTracking()
            .Include(u => u.LinkedPatient)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        return user?.LinkedPatient is null ? null : ToProfileResponse(user.LinkedPatient);
    }

    public async Task<PortalProfileResponse?> UpdateProfileAsync(Guid userId, UpdatePortalProfileRequest request, CancellationToken cancellationToken = default)
    {
        var user = await dbContext.Users
            .Include(u => u.LinkedPatient)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user?.LinkedPatient is null) return null;

        var patient = user.LinkedPatient;
        patient.PhoneNumber = request.PhoneNumber;
        patient.Gender = request.Gender;
        patient.BloodGroup = request.BloodGroup;
        patient.AddressLine1 = request.AddressLine1;
        patient.City = request.City;
        patient.PostalCode = request.PostalCode;
        patient.Country = request.Country;
        patient.EmergencyContactName = request.EmergencyContactName;
        patient.EmergencyContactPhone = request.EmergencyContactPhone;
        patient.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        return ToProfileResponse(patient);
    }

    public async Task<PortalDashboardResponse?> GetDashboardAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await dbContext.Users
            .AsNoTracking()
            .Include(u => u.LinkedPatient)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user?.LinkedPatient is null) return null;

        var patientId = user.LinkedPatient.Id;

        var upcomingAppointments = await dbContext.Appointments
            .AsNoTracking()
            .Include(a => a.DoctorUser)
            .Where(a => a.PatientId == patientId
                     && a.ScheduledAtUtc >= DateTime.UtcNow
                     && a.Status != AppointmentStatus.Cancelled
                     && a.Status != AppointmentStatus.NoShow)
            .OrderBy(a => a.ScheduledAtUtc)
            .Take(5)
            .Select(a => ToAppointmentResponse(a))
            .ToListAsync(cancellationToken);

        var activePrescriptions = await dbContext.Prescriptions
            .AsNoTracking()
            .Include(p => p.Medication)
            .Where(p => p.PatientId == patientId && p.Status == PrescriptionStatus.Active)
            .OrderByDescending(p => p.PrescribedAtUtc)
            .Take(5)
            .Select(p => ToPrescriptionResponse(p))
            .ToListAsync(cancellationToken);

        var totalEncounters = await dbContext.ClinicalEncounters
            .AsNoTracking()
            .CountAsync(e => e.PatientId == patientId, cancellationToken);

        return new PortalDashboardResponse(
            ToProfileResponse(user.LinkedPatient),
            upcomingAppointments,
            activePrescriptions,
            totalEncounters);
    }

    public async Task<IReadOnlyList<PortalAppointmentResponse>> GetAppointmentsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var patientId = await ResolvePatientIdAsync(userId, cancellationToken);
        if (patientId is null) return [];

        return await dbContext.Appointments
            .AsNoTracking()
            .Include(a => a.DoctorUser)
            .Where(a => a.PatientId == patientId.Value)
            .OrderByDescending(a => a.ScheduledAtUtc)
            .Select(a => ToAppointmentResponse(a))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PortalEncounterResponse>> GetEncountersAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var patientId = await ResolvePatientIdAsync(userId, cancellationToken);
        if (patientId is null) return [];

        return await dbContext.ClinicalEncounters
            .AsNoTracking()
            .Include(e => e.AttendingDoctor)
            .Where(e => e.PatientId == patientId.Value)
            .OrderByDescending(e => e.StartedAtUtc)
            .Select(e => ToEncounterResponse(e))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PortalPrescriptionResponse>> GetPrescriptionsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var patientId = await ResolvePatientIdAsync(userId, cancellationToken);
        if (patientId is null) return [];

        return await dbContext.Prescriptions
            .AsNoTracking()
            .Include(p => p.Medication)
            .Where(p => p.PatientId == patientId.Value)
            .OrderByDescending(p => p.PrescribedAtUtc)
            .Select(p => ToPrescriptionResponse(p))
            .ToListAsync(cancellationToken);
    }

    public async Task LogSessionAsync(LogPortalSessionRequest request, CancellationToken cancellationToken = default)
    {
        var session = new PatientPortalSession
        {
            UserId = request.UserId,
            PatientId = request.PatientId,
            IpAddress = request.IpAddress,
            UserAgent = request.UserAgent
        };
        dbContext.PatientPortalSessions.Add(session);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<Guid?> ResolvePatientIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        var linkedPatientId = await dbContext.Users
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => u.LinkedPatientId)
            .FirstOrDefaultAsync(cancellationToken);
        return linkedPatientId;
    }

    private static PortalProfileResponse ToProfileResponse(Data.Persistence.Entities.Patient p) => new(
        p.Id, p.FirstName, p.LastName, p.MedicalRecordNumber, p.DateOfBirth,
        p.Email, p.PhoneNumber, p.Gender, p.BloodGroup,
        p.AddressLine1, p.City, p.PostalCode, p.Country,
        p.EmergencyContactName, p.EmergencyContactPhone);

    private static PortalAppointmentResponse ToAppointmentResponse(Appointment a) => new(
        a.Id, a.ScheduledAtUtc, a.DurationMinutes, a.Status, a.Type,
        a.Reason, a.Notes, a.DoctorUser.Username);

    private static PortalEncounterResponse ToEncounterResponse(ClinicalEncounter e) => new(
        e.Id, e.StartedAtUtc, e.EndedAtUtc, e.EncounterType,
        e.ChiefComplaint, e.Assessment, e.Plan, e.FollowUpNotes,
        e.IsClosed, e.AttendingDoctor.Username);

    private static PortalPrescriptionResponse ToPrescriptionResponse(Prescription p) => new(
        p.Id, p.Medication.GenericName, p.Medication.BrandName, p.Medication.Form,
        p.Medication.Strength, p.Medication.RouteOfAdministration,
        p.Dose, p.Frequency, p.DurationDays, p.Instructions,
        p.Status, p.Medication.IsControlled, p.PrescribedAtUtc, p.DispensedAtUtc);
}
