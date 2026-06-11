using HospitalMS.Business.Models;
using HospitalMS.Data.Persistence;
using HospitalMS.Data.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace HospitalMS.Business.Services;

public sealed class EncounterService(HospitalDbContext dbContext) : IEncounterService
{
    public async Task<IReadOnlyList<EncounterResponse>> GetAllAsync(
        Guid? patientId = null,
        bool? isClosed = null,
        Guid? attendingDoctorId = null,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.ClinicalEncounters
            .AsNoTracking()
            .Include(e => e.Patient)
            .Include(e => e.AttendingDoctor)
            .Include(e => e.Diagnoses)
            .Include(e => e.VitalSigns).ThenInclude(v => v.RecordedByUser)
            .AsQueryable();

        if (patientId.HasValue)
            query = query.Where(e => e.PatientId == patientId.Value);
        if (isClosed.HasValue)
            query = query.Where(e => e.IsClosed == isClosed.Value);
        if (attendingDoctorId.HasValue)
            query = query.Where(e => e.AttendingDoctorId == attendingDoctorId.Value);

        var encounters = await query
            .OrderByDescending(e => e.StartedAtUtc)
            .ToListAsync(cancellationToken);

        return encounters.Select(ToResponse).ToArray();
    }

    public async Task<EncounterResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var encounter = await dbContext.ClinicalEncounters
            .AsNoTracking()
            .Include(e => e.Patient)
            .Include(e => e.AttendingDoctor)
            .Include(e => e.Diagnoses)
            .Include(e => e.VitalSigns).ThenInclude(v => v.RecordedByUser)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

        return encounter is null ? null : ToResponse(encounter);
    }

    public async Task<EncounterResponse> CreateAsync(CreateEncounterRequest request, CancellationToken cancellationToken = default)
    {
        var encounter = new ClinicalEncounter
        {
            PatientId = request.PatientId,
            AppointmentId = request.AppointmentId,
            AttendingDoctorId = request.AttendingDoctorId,
            EncounterType = request.EncounterType,
            ChiefComplaint = request.ChiefComplaint,
            HistoryOfPresentIllness = request.HistoryOfPresentIllness,
            Examination = request.Examination,
            Assessment = request.Assessment,
            Plan = request.Plan
        };

        dbContext.ClinicalEncounters.Add(encounter);
        await dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(encounter.Id, cancellationToken)
            ?? throw new InvalidOperationException("Encounter was not found after creation.");
    }

    public async Task<EncounterResponse> UpdateAsync(Guid id, UpdateEncounterRequest request, CancellationToken cancellationToken = default)
    {
        var encounter = await dbContext.ClinicalEncounters.FindAsync([id], cancellationToken)
            ?? throw new KeyNotFoundException($"Encounter '{id}' was not found.");

        if (encounter.IsClosed)
            throw new InvalidOperationException("Cannot update a closed encounter.");

        encounter.EncounterType = request.EncounterType;
        encounter.ChiefComplaint = request.ChiefComplaint;
        encounter.HistoryOfPresentIllness = request.HistoryOfPresentIllness;
        encounter.Examination = request.Examination;
        encounter.Assessment = request.Assessment;
        encounter.Plan = request.Plan;
        encounter.FollowUpNotes = request.FollowUpNotes;
        encounter.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException("Encounter was not found after update.");
    }

    public async Task<EncounterResponse> CloseAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var encounter = await dbContext.ClinicalEncounters.FindAsync([id], cancellationToken)
            ?? throw new KeyNotFoundException($"Encounter '{id}' was not found.");

        if (encounter.IsClosed)
            throw new InvalidOperationException("Encounter is already closed.");

        encounter.IsClosed = true;
        encounter.EndedAtUtc = DateTime.UtcNow;
        encounter.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException("Encounter was not found after close.");
    }

    public async Task<DiagnosisResponse> AddDiagnosisAsync(Guid encounterId, AddDiagnosisRequest request, CancellationToken cancellationToken = default)
    {
        var encounter = await dbContext.ClinicalEncounters.FindAsync([encounterId], cancellationToken)
            ?? throw new KeyNotFoundException($"Encounter '{encounterId}' was not found.");

        if (encounter.IsClosed)
            throw new InvalidOperationException("Cannot add a diagnosis to a closed encounter.");

        var diagnosis = new Diagnosis
        {
            EncounterId = encounterId,
            IcdCode = request.IcdCode,
            Description = request.Description,
            DiagnosisType = request.DiagnosisType
        };

        dbContext.Diagnoses.Add(diagnosis);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ToDiagnosisResponse(diagnosis);
    }

    public async Task DeleteDiagnosisAsync(Guid encounterId, Guid diagnosisId, CancellationToken cancellationToken = default)
    {
        var diagnosis = await dbContext.Diagnoses
            .FirstOrDefaultAsync(d => d.Id == diagnosisId && d.EncounterId == encounterId, cancellationToken)
            ?? throw new KeyNotFoundException($"Diagnosis '{diagnosisId}' was not found on encounter '{encounterId}'.");

        dbContext.Diagnoses.Remove(diagnosis);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<VitalsResponse> AddVitalsAsync(Guid encounterId, Guid recordedByUserId, AddVitalsRequest request, CancellationToken cancellationToken = default)
    {
        var encounter = await dbContext.ClinicalEncounters.FindAsync([encounterId], cancellationToken)
            ?? throw new KeyNotFoundException($"Encounter '{encounterId}' was not found.");

        if (encounter.IsClosed)
            throw new InvalidOperationException("Cannot add vitals to a closed encounter.");

        var vitals = new VitalSigns
        {
            EncounterId = encounterId,
            RecordedByUserId = recordedByUserId,
            HeightCm = request.HeightCm,
            WeightKg = request.WeightKg,
            TemperatureCelsius = request.TemperatureCelsius,
            BloodPressureSystolic = request.BloodPressureSystolic,
            BloodPressureDiastolic = request.BloodPressureDiastolic,
            HeartRateBpm = request.HeartRateBpm,
            RespiratoryRate = request.RespiratoryRate,
            OxygenSaturationPct = request.OxygenSaturationPct,
            Notes = request.Notes
        };

        dbContext.VitalSigns.Add(vitals);
        await dbContext.SaveChangesAsync(cancellationToken);

        var user = await dbContext.Users.FindAsync([recordedByUserId], cancellationToken);
        vitals.RecordedByUser = user!;

        return ToVitalsResponse(vitals);
    }

    private static EncounterResponse ToResponse(ClinicalEncounter e) => new(
        e.Id,
        e.PatientId,
        $"{e.Patient.FirstName} {e.Patient.LastName}",
        e.Patient.MedicalRecordNumber,
        e.AppointmentId,
        e.AttendingDoctorId,
        e.AttendingDoctor.Username,
        e.EncounterType,
        e.StartedAtUtc,
        e.EndedAtUtc,
        e.ChiefComplaint,
        e.HistoryOfPresentIllness,
        e.Examination,
        e.Assessment,
        e.Plan,
        e.FollowUpNotes,
        e.IsClosed,
        e.CreatedAtUtc,
        e.UpdatedAtUtc,
        e.Diagnoses.OrderBy(d => d.CreatedAtUtc).Select(ToDiagnosisResponse).ToArray(),
        e.VitalSigns.OrderByDescending(v => v.RecordedAtUtc).Select(ToVitalsResponse).ToArray());

    private static DiagnosisResponse ToDiagnosisResponse(Diagnosis d) => new(
        d.Id,
        d.EncounterId,
        d.IcdCode,
        d.Description,
        d.DiagnosisType,
        d.CreatedAtUtc);

    private static VitalsResponse ToVitalsResponse(VitalSigns v) => new(
        v.Id,
        v.EncounterId,
        v.RecordedByUserId,
        v.RecordedByUser?.Username ?? string.Empty,
        v.RecordedAtUtc,
        v.HeightCm,
        v.WeightKg,
        v.TemperatureCelsius,
        v.BloodPressureSystolic,
        v.BloodPressureDiastolic,
        v.HeartRateBpm,
        v.RespiratoryRate,
        v.OxygenSaturationPct,
        v.Notes);
}
