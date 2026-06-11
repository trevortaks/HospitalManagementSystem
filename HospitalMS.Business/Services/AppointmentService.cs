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
            .Include(a => a.PreConsultVitals).ThenInclude(v => v!.RecordedByUser)
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
            .Include(a => a.PreConsultVitals).ThenInclude(v => v!.RecordedByUser)
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

    public async Task<AppointmentVitalsResponse> RecordVitalsAsync(
        Guid appointmentId, Guid recordedByUserId,
        RecordAppointmentVitalsRequest request, CancellationToken cancellationToken = default)
    {
        var appointment = await dbContext.Appointments.FindAsync([appointmentId], cancellationToken)
            ?? throw new KeyNotFoundException($"Appointment '{appointmentId}' was not found.");

        var existing = await dbContext.AppointmentVitals
            .FirstOrDefaultAsync(v => v.AppointmentId == appointmentId, cancellationToken);

        if (existing is not null)
        {
            existing.HeightCm              = request.HeightCm ?? existing.HeightCm;
            existing.WeightKg              = request.WeightKg ?? existing.WeightKg;
            existing.TemperatureCelsius    = request.TemperatureCelsius ?? existing.TemperatureCelsius;
            existing.BloodPressureSystolic = request.BloodPressureSystolic ?? existing.BloodPressureSystolic;
            existing.BloodPressureDiastolic= request.BloodPressureDiastolic ?? existing.BloodPressureDiastolic;
            existing.HeartRateBpm          = request.HeartRateBpm ?? existing.HeartRateBpm;
            existing.RespiratoryRate       = request.RespiratoryRate ?? existing.RespiratoryRate;
            existing.OxygenSaturationPct   = request.OxygenSaturationPct ?? existing.OxygenSaturationPct;
            existing.Notes                 = request.Notes ?? existing.Notes;
            existing.RecordedAtUtc         = DateTime.UtcNow;
            existing.RecordedByUserId      = recordedByUserId;
        }
        else
        {
            existing = new AppointmentVitals
            {
                AppointmentId         = appointmentId,
                RecordedByUserId      = recordedByUserId,
                HeightCm              = request.HeightCm,
                WeightKg              = request.WeightKg,
                TemperatureCelsius    = request.TemperatureCelsius,
                BloodPressureSystolic = request.BloodPressureSystolic,
                BloodPressureDiastolic= request.BloodPressureDiastolic,
                HeartRateBpm          = request.HeartRateBpm,
                RespiratoryRate       = request.RespiratoryRate,
                OxygenSaturationPct   = request.OxygenSaturationPct,
                Notes                 = request.Notes
            };
            dbContext.AppointmentVitals.Add(existing);

            if (appointment.Status == AppointmentStatus.Scheduled || appointment.Status == AppointmentStatus.Confirmed)
            {
                appointment.Status = AppointmentStatus.InProgress;
                appointment.UpdatedAtUtc = DateTime.UtcNow;
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return await GetVitalsResponseAsync(existing.Id, cancellationToken);
    }

    public async Task<AppointmentVitalsResponse?> GetVitalsAsync(Guid appointmentId, CancellationToken cancellationToken = default)
    {
        var vitals = await dbContext.AppointmentVitals
            .AsNoTracking()
            .Include(v => v.RecordedByUser)
            .FirstOrDefaultAsync(v => v.AppointmentId == appointmentId, cancellationToken);

        return vitals is null ? null : ToVitalsResponse(vitals);
    }

    private async Task<AppointmentVitalsResponse> GetVitalsResponseAsync(Guid vitalsId, CancellationToken ct)
    {
        var vitals = await dbContext.AppointmentVitals
            .AsNoTracking()
            .Include(v => v.RecordedByUser)
            .FirstAsync(v => v.Id == vitalsId, ct);
        return ToVitalsResponse(vitals);
    }

    private static AppointmentVitalsResponse ToVitalsResponse(AppointmentVitals v) => new(
        v.Id, v.AppointmentId, v.RecordedByUserId,
        v.RecordedByUser?.Username ?? string.Empty,
        v.RecordedAtUtc,
        v.HeightCm, v.WeightKg, v.TemperatureCelsius,
        v.BloodPressureSystolic, v.BloodPressureDiastolic,
        v.HeartRateBpm, v.RespiratoryRate, v.OxygenSaturationPct, v.Notes);

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
        a.UpdatedAtUtc,
        a.PreConsultVitals is null ? null : ToVitalsResponse(a.PreConsultVitals));
}
