using HospitalMS.Business.Models;
using HospitalMS.Data.Persistence;
using HospitalMS.Data.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace HospitalMS.Business.Services;

public sealed class MedicationService(HospitalDbContext dbContext) : IMedicationService
{
    public async Task<IReadOnlyList<MedicationResponse>> GetAllAsync(bool? activeOnly = null, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Medications.AsNoTracking().AsQueryable();

        if (activeOnly == true)
            query = query.Where(m => m.IsActive);

        var meds = await query.OrderBy(m => m.GenericName).ToListAsync(cancellationToken);
        return meds.Select(ToResponse).ToArray();
    }

    public async Task<MedicationResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var med = await dbContext.Medications.AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
        return med is null ? null : ToResponse(med);
    }

    public async Task<MedicationResponse> CreateAsync(CreateMedicationRequest request, CancellationToken cancellationToken = default)
    {
        var med = new Medication
        {
            GenericName = request.GenericName,
            BrandName = request.BrandName,
            Form = request.Form,
            Strength = request.Strength,
            RouteOfAdministration = request.RouteOfAdministration,
            IsControlled = request.IsControlled,
            IsActive = request.IsActive
        };
        dbContext.Medications.Add(med);
        await dbContext.SaveChangesAsync(cancellationToken);
        return ToResponse(med);
    }

    public async Task<MedicationResponse> UpdateAsync(Guid id, UpdateMedicationRequest request, CancellationToken cancellationToken = default)
    {
        var med = await dbContext.Medications.FindAsync([id], cancellationToken)
            ?? throw new KeyNotFoundException($"Medication '{id}' was not found.");

        med.GenericName = request.GenericName;
        med.BrandName = request.BrandName;
        med.Form = request.Form;
        med.Strength = request.Strength;
        med.RouteOfAdministration = request.RouteOfAdministration;
        med.IsControlled = request.IsControlled;
        med.IsActive = request.IsActive;

        await dbContext.SaveChangesAsync(cancellationToken);
        return ToResponse(med);
    }

    public async Task<MedicationResponse> ToggleActiveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var med = await dbContext.Medications.FindAsync([id], cancellationToken)
            ?? throw new KeyNotFoundException($"Medication '{id}' was not found.");

        med.IsActive = !med.IsActive;
        await dbContext.SaveChangesAsync(cancellationToken);
        return ToResponse(med);
    }

    private static MedicationResponse ToResponse(Medication m) => new(
        m.Id, m.GenericName, m.BrandName, m.Form, m.Strength,
        m.RouteOfAdministration, m.IsControlled, m.IsActive, m.CreatedAtUtc);
}
