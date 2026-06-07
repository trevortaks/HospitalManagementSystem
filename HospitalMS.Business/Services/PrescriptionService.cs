using HospitalMS.Business.Models;
using HospitalMS.Data.Persistence;
using HospitalMS.Data.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace HospitalMS.Business.Services;

public sealed class PrescriptionService(HospitalDbContext dbContext) : IPrescriptionService
{
    public async Task<IReadOnlyList<PrescriptionResponse>> GetAllAsync(
        Guid? patientId = null,
        Guid? encounterId = null,
        string? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Prescriptions
            .AsNoTracking()
            .Include(p => p.Patient)
            .Include(p => p.Medication)
            .Include(p => p.PrescribedByUser)
            .Include(p => p.DispensedByUser)
            .AsQueryable();

        if (patientId.HasValue)
            query = query.Where(p => p.PatientId == patientId.Value);
        if (encounterId.HasValue)
            query = query.Where(p => p.EncounterId == encounterId.Value);
        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(p => p.Status == status);

        var prescriptions = await query
            .OrderByDescending(p => p.PrescribedAtUtc)
            .ToListAsync(cancellationToken);

        return prescriptions.Select(ToResponse).ToArray();
    }

    public async Task<PrescriptionResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var prescription = await dbContext.Prescriptions
            .AsNoTracking()
            .Include(p => p.Patient)
            .Include(p => p.Medication)
            .Include(p => p.PrescribedByUser)
            .Include(p => p.DispensedByUser)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        return prescription is null ? null : ToResponse(prescription);
    }

    public async Task<PrescriptionResponse> CreateAsync(CreatePrescriptionRequest request, CancellationToken cancellationToken = default)
    {
        var prescription = new Prescription
        {
            EncounterId = request.EncounterId,
            PatientId = request.PatientId,
            PrescribedByUserId = request.PrescribedByUserId,
            MedicationId = request.MedicationId,
            Dose = request.Dose,
            Frequency = request.Frequency,
            DurationDays = request.DurationDays,
            Instructions = request.Instructions,
            Status = PrescriptionStatus.Active
        };

        dbContext.Prescriptions.Add(prescription);
        await dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(prescription.Id, cancellationToken)
            ?? throw new InvalidOperationException("Prescription not found after creation.");
    }

    public async Task<PrescriptionResponse> DispenseAsync(Guid id, DispensePrescriptionRequest request, CancellationToken cancellationToken = default)
    {
        var prescription = await dbContext.Prescriptions.FindAsync([id], cancellationToken)
            ?? throw new KeyNotFoundException($"Prescription '{id}' was not found.");

        if (prescription.Status != PrescriptionStatus.Active)
            throw new InvalidOperationException($"Cannot dispense a prescription with status '{prescription.Status}'.");

        prescription.Status = PrescriptionStatus.Dispensed;
        prescription.QuantityDispensed = request.QuantityDispensed;
        prescription.DispensedByUserId = request.DispensedByUserId;
        prescription.DispensedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException("Prescription not found after dispense.");
    }

    public async Task<PrescriptionResponse> CancelAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var prescription = await dbContext.Prescriptions.FindAsync([id], cancellationToken)
            ?? throw new KeyNotFoundException($"Prescription '{id}' was not found.");

        if (prescription.Status == PrescriptionStatus.Dispensed)
            throw new InvalidOperationException("Cannot cancel a prescription that has already been dispensed.");

        prescription.Status = PrescriptionStatus.Cancelled;
        await dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException("Prescription not found after cancel.");
    }

    private static PrescriptionResponse ToResponse(Prescription p) => new(
        p.Id,
        p.EncounterId,
        p.PatientId,
        $"{p.Patient.FirstName} {p.Patient.LastName}",
        p.Patient.MedicalRecordNumber,
        p.PrescribedByUserId,
        p.PrescribedByUser.Username,
        p.MedicationId,
        p.Medication.GenericName,
        p.Medication.BrandName,
        p.Medication.Form,
        p.Medication.Strength,
        p.Medication.RouteOfAdministration,
        p.Dose,
        p.Frequency,
        p.DurationDays,
        p.QuantityDispensed,
        p.Instructions,
        p.Status,
        p.Medication.IsControlled,
        p.PrescribedAtUtc,
        p.DispensedAtUtc,
        p.DispensedByUser?.Username);
}
