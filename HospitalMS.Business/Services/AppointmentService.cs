using HospitalMS.Business.Models;
using HospitalMS.Data.Persistence;
using HospitalMS.Data.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace HospitalMS.Business.Services;

public sealed class AppointmentService(HospitalDbContext dbContext) : IAppointmentService
{
    public async Task<IReadOnlyList<AppointmentResponse>> GetAllAsync(
        Guid? patientId = null,
        Guid? doctorUserId = null,
        string? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Appointments
            .AsNoTracking()
            .Include(a => a.Patient)
            .Include(a => a.DoctorUser)
            .AsQueryable();

        if (patientId.HasValue)
            query = query.Where(a => a.PatientId == patientId.Value);
        if (doctorUserId.HasValue)
            query = query.Where(a => a.DoctorUserId == doctorUserId.Value);
        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(a => a.Status == status);

        var appointments = await query
            .OrderByDescending(a => a.ScheduledAtUtc)
            .ToListAsync(cancellationToken);

        return appointments.Select(ToResponse).ToArray();
    }

    public async Task<AppointmentResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var appointment = await dbContext.Appointments
            .AsNoTracking()
            .Include(a => a.Patient)
            .Include(a => a.DoctorUser)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        return appointment is null ? null : ToResponse(appointment);
    }

    public async Task<AppointmentResponse> CreateAsync(CreateAppointmentRequest request, CancellationToken cancellationToken = default)
    {
        var appointment = new Appointment
        {
            PatientId = request.PatientId,
            DoctorUserId = request.DoctorUserId,
            ScheduledAtUtc = request.ScheduledAtUtc,
            DurationMinutes = request.DurationMinutes,
            Type = request.Type,
            Reason = request.Reason,
            Notes = request.Notes,
            Status = AppointmentStatus.Scheduled
        };

        dbContext.Appointments.Add(appointment);
        await dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(appointment.Id, cancellationToken)
            ?? throw new InvalidOperationException("Appointment was not found after creation.");
    }

    public async Task<AppointmentResponse> UpdateAsync(Guid id, UpdateAppointmentRequest request, CancellationToken cancellationToken = default)
    {
        var appointment = await dbContext.Appointments.FindAsync([id], cancellationToken)
            ?? throw new KeyNotFoundException($"Appointment '{id}' was not found.");

        appointment.ScheduledAtUtc = request.ScheduledAtUtc;
        appointment.DurationMinutes = request.DurationMinutes;
        appointment.Type = request.Type;
        appointment.Reason = request.Reason;
        appointment.Notes = request.Notes;
        appointment.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException("Appointment was not found after update.");
    }

    public async Task<AppointmentResponse> CancelAsync(Guid id, CancelAppointmentRequest request, CancellationToken cancellationToken = default)
    {
        var appointment = await dbContext.Appointments.FindAsync([id], cancellationToken)
            ?? throw new KeyNotFoundException($"Appointment '{id}' was not found.");

        appointment.Status = AppointmentStatus.Cancelled;
        appointment.CancelledReason = request.Reason;
        appointment.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException("Appointment was not found after cancel.");
    }

    public async Task<AppointmentResponse> PatchStatusAsync(Guid id, PatchAppointmentStatusRequest request, CancellationToken cancellationToken = default)
    {
        var appointment = await dbContext.Appointments.FindAsync([id], cancellationToken)
            ?? throw new KeyNotFoundException($"Appointment '{id}' was not found.");

        appointment.Status = request.Status;
        appointment.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException("Appointment was not found after status patch.");
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var appointment = await dbContext.Appointments.FindAsync([id], cancellationToken)
            ?? throw new KeyNotFoundException($"Appointment '{id}' was not found.");

        dbContext.Appointments.Remove(appointment);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static AppointmentResponse ToResponse(Appointment a) => new(
        a.Id,
        a.PatientId,
        $"{a.Patient.FirstName} {a.Patient.LastName}",
        a.Patient.MedicalRecordNumber,
        a.DoctorUserId,
        a.DoctorUser.Username,
        a.ScheduledAtUtc,
        a.DurationMinutes,
        a.Status,
        a.Type,
        a.Reason,
        a.Notes,
        a.CancelledReason,
        a.CreatedAtUtc,
        a.UpdatedAtUtc);
}
