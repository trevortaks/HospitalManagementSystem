using HospitalMS.Business.Models;
using HospitalMS.Data.Persistence;
using HospitalMS.Data.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace HospitalMS.Business.Services;

public sealed class BedManagementService(HospitalDbContext dbContext) : IBedManagementService
{
    // ── Wards ────────────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<WardResponse>> GetAllWardsAsync(bool? activeOnly = null, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Wards.AsNoTracking()
            .Include(w => w.Beds)
            .AsQueryable();
        if (activeOnly == true) query = query.Where(w => w.IsActive);
        var wards = await query.OrderBy(w => w.Name).ToListAsync(cancellationToken);
        return wards.Select(ToWardResponse).ToArray();
    }

    public async Task<WardResponse?> GetWardByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var ward = await dbContext.Wards.AsNoTracking()
            .Include(w => w.Beds)
            .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);
        return ward is null ? null : ToWardResponse(ward);
    }

    public async Task<WardResponse> CreateWardAsync(CreateWardRequest request, CancellationToken cancellationToken = default)
    {
        var ward = new Ward
        {
            Name = request.Name,
            WardType = request.WardType,
            TotalBeds = request.TotalBeds,
            FloorNumber = request.FloorNumber
        };
        dbContext.Wards.Add(ward);
        await dbContext.SaveChangesAsync(cancellationToken);
        return ToWardResponse(await LoadWardAsync(ward.Id, cancellationToken)!);
    }

    public async Task<WardResponse> UpdateWardAsync(Guid id, UpdateWardRequest request, CancellationToken cancellationToken = default)
    {
        var ward = await dbContext.Wards.FindAsync([id], cancellationToken)
            ?? throw new KeyNotFoundException($"Ward '{id}' not found.");
        ward.Name = request.Name;
        ward.WardType = request.WardType;
        ward.TotalBeds = request.TotalBeds;
        ward.FloorNumber = request.FloorNumber;
        await dbContext.SaveChangesAsync(cancellationToken);
        return ToWardResponse(await LoadWardAsync(id, cancellationToken)!);
    }

    public async Task<WardResponse> ToggleWardActiveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var ward = await dbContext.Wards.FindAsync([id], cancellationToken)
            ?? throw new KeyNotFoundException($"Ward '{id}' not found.");
        ward.IsActive = !ward.IsActive;
        await dbContext.SaveChangesAsync(cancellationToken);
        return ToWardResponse(await LoadWardAsync(id, cancellationToken)!);
    }

    // ── Beds ─────────────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<BedResponse>> GetBedsByWardAsync(Guid wardId, CancellationToken cancellationToken = default)
    {
        var beds = await dbContext.Beds.AsNoTracking()
            .Include(b => b.Ward)
            .Where(b => b.WardId == wardId)
            .OrderBy(b => b.BedNumber)
            .ToListAsync(cancellationToken);
        return beds.Select(ToBedResponse).ToArray();
    }

    public async Task<BedResponse?> GetBedByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var bed = await dbContext.Beds.AsNoTracking()
            .Include(b => b.Ward)
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
        return bed is null ? null : ToBedResponse(bed);
    }

    public async Task<BedResponse> CreateBedAsync(CreateBedRequest request, CancellationToken cancellationToken = default)
    {
        _ = await dbContext.Wards.FindAsync([request.WardId], cancellationToken)
            ?? throw new KeyNotFoundException($"Ward '{request.WardId}' not found.");

        var bed = new Bed
        {
            WardId = request.WardId,
            BedNumber = request.BedNumber,
            BedType = request.BedType
        };
        dbContext.Beds.Add(bed);
        await dbContext.SaveChangesAsync(cancellationToken);
        return ToBedResponse(await LoadBedAsync(bed.Id, cancellationToken)!);
    }

    public async Task<BedResponse> UpdateBedStatusAsync(Guid id, string status, CancellationToken cancellationToken = default)
    {
        var bed = await dbContext.Beds.FindAsync([id], cancellationToken)
            ?? throw new KeyNotFoundException($"Bed '{id}' not found.");
        bed.Status = status;
        await dbContext.SaveChangesAsync(cancellationToken);
        return ToBedResponse(await LoadBedAsync(id, cancellationToken)!);
    }

    public async Task<BedResponse> ToggleBedActiveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var bed = await dbContext.Beds.FindAsync([id], cancellationToken)
            ?? throw new KeyNotFoundException($"Bed '{id}' not found.");
        bed.IsActive = !bed.IsActive;
        await dbContext.SaveChangesAsync(cancellationToken);
        return ToBedResponse(await LoadBedAsync(id, cancellationToken)!);
    }

    // ── Allocations ──────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<BedAllocationResponse>> GetActiveAllocationsAsync(Guid? wardId = null, CancellationToken cancellationToken = default)
    {
        var query = dbContext.BedAllocations.AsNoTracking()
            .Include(a => a.Bed).ThenInclude(b => b.Ward)
            .Include(a => a.Patient)
            .Include(a => a.AdmittedByUser)
            .Include(a => a.DischargedByUser)
            .Where(a => a.DischargedAtUtc == null);

        if (wardId.HasValue)
            query = query.Where(a => a.Bed.WardId == wardId.Value);

        var allocations = await query.OrderBy(a => a.AdmittedAtUtc).ToListAsync(cancellationToken);
        return allocations.Select(ToAllocationResponse).ToArray();
    }

    public async Task<BedAllocationResponse?> GetPatientCurrentAllocationAsync(Guid patientId, CancellationToken cancellationToken = default)
    {
        var allocation = await dbContext.BedAllocations.AsNoTracking()
            .Include(a => a.Bed).ThenInclude(b => b.Ward)
            .Include(a => a.Patient)
            .Include(a => a.AdmittedByUser)
            .Include(a => a.DischargedByUser)
            .FirstOrDefaultAsync(a => a.PatientId == patientId && a.DischargedAtUtc == null, cancellationToken);
        return allocation is null ? null : ToAllocationResponse(allocation);
    }

    public async Task<BedAllocationResponse> AllocateBedAsync(AllocateBedRequest request, CancellationToken cancellationToken = default)
    {
        var bed = await dbContext.Beds.FindAsync([request.BedId], cancellationToken)
            ?? throw new KeyNotFoundException($"Bed '{request.BedId}' not found.");

        if (bed.Status != BedStatus.Available)
            throw new InvalidOperationException($"Bed is currently {bed.Status} and cannot be allocated.");

        var existingAllocation = await dbContext.BedAllocations
            .FirstOrDefaultAsync(a => a.BedId == request.BedId && a.DischargedAtUtc == null, cancellationToken);
        if (existingAllocation is not null)
            throw new InvalidOperationException("Bed already has an active allocation.");

        var allocation = new BedAllocation
        {
            BedId = request.BedId,
            PatientId = request.PatientId,
            EncounterId = request.EncounterId,
            AdmittedByUserId = request.AdmittedByUserId
        };
        dbContext.BedAllocations.Add(allocation);

        bed.Status = BedStatus.Occupied;
        await dbContext.SaveChangesAsync(cancellationToken);

        return ToAllocationResponse(await LoadAllocationAsync(allocation.Id, cancellationToken)!);
    }

    public async Task<BedAllocationResponse> DischargePatientAsync(DischargePatientRequest request, CancellationToken cancellationToken = default)
    {
        var allocation = await dbContext.BedAllocations
            .Include(a => a.Bed)
            .FirstOrDefaultAsync(a => a.Id == request.AllocationId, cancellationToken)
            ?? throw new KeyNotFoundException($"Allocation '{request.AllocationId}' not found.");

        if (allocation.DischargedAtUtc.HasValue)
            throw new InvalidOperationException("Patient has already been discharged.");

        allocation.DischargedAtUtc = DateTime.UtcNow;
        allocation.DischargeReason = request.DischargeReason;
        allocation.DischargedByUserId = request.DischargedByUserId;
        allocation.TransferredToBedId = request.TransferToBedId;

        allocation.Bed.Status = BedStatus.Cleaning;

        if (request.TransferToBedId.HasValue)
        {
            var targetBed = await dbContext.Beds.FindAsync([request.TransferToBedId.Value], cancellationToken)
                ?? throw new KeyNotFoundException($"Transfer bed '{request.TransferToBedId}' not found.");

            if (targetBed.Status != BedStatus.Available)
                throw new InvalidOperationException($"Transfer bed is {targetBed.Status}; cannot transfer.");

            var transferAllocation = new BedAllocation
            {
                BedId = targetBed.Id,
                PatientId = allocation.PatientId,
                EncounterId = allocation.EncounterId,
                AdmittedByUserId = request.DischargedByUserId
            };
            dbContext.BedAllocations.Add(transferAllocation);
            targetBed.Status = BedStatus.Occupied;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return ToAllocationResponse(await LoadAllocationAsync(allocation.Id, cancellationToken)!);
    }

    // ── Availability ─────────────────────────────────────────────────────────

    public async Task<BedAvailabilityResponse> GetBedAvailabilityAsync(CancellationToken cancellationToken = default)
    {
        var beds = await dbContext.Beds.AsNoTracking()
            .Where(b => b.IsActive)
            .Select(b => b.Status)
            .ToListAsync(cancellationToken);

        var total = beds.Count;
        var occupied = beds.Count(s => s == BedStatus.Occupied);
        var available = beds.Count(s => s == BedStatus.Available);
        var outOfService = beds.Count(s => s is BedStatus.OutOfService or BedStatus.Cleaning);
        var rate = total > 0 ? Math.Round((double)occupied / total * 100, 1) : 0.0;

        return new BedAvailabilityResponse(total, occupied, available, outOfService, rate);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private async Task<Ward?> LoadWardAsync(Guid id, CancellationToken cancellationToken) =>
        await dbContext.Wards.AsNoTracking()
            .Include(w => w.Beds)
            .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);

    private async Task<Bed?> LoadBedAsync(Guid id, CancellationToken cancellationToken) =>
        await dbContext.Beds.AsNoTracking()
            .Include(b => b.Ward)
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

    private async Task<BedAllocation?> LoadAllocationAsync(Guid id, CancellationToken cancellationToken) =>
        await dbContext.BedAllocations.AsNoTracking()
            .Include(a => a.Bed).ThenInclude(b => b.Ward)
            .Include(a => a.Patient)
            .Include(a => a.AdmittedByUser)
            .Include(a => a.DischargedByUser)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    private static WardResponse ToWardResponse(Ward w)
    {
        var occupied = w.Beds.Count(b => b.Status == BedStatus.Occupied);
        var available = w.Beds.Count(b => b.Status == BedStatus.Available);
        return new WardResponse(w.Id, w.Name, w.WardType, w.TotalBeds, w.FloorNumber,
            w.IsActive, w.CreatedAtUtc, occupied, available);
    }

    private static BedResponse ToBedResponse(Bed b) =>
        new(b.Id, b.WardId, b.Ward.Name, b.BedNumber, b.BedType, b.Status, b.IsActive, b.CreatedAtUtc);

    private static BedAllocationResponse ToAllocationResponse(BedAllocation a) =>
        new(a.Id, a.BedId, a.Bed.BedNumber, a.Bed.Ward.Name,
            a.PatientId, $"{a.Patient.FirstName} {a.Patient.LastName}", a.Patient.MedicalRecordNumber,
            a.EncounterId, a.AdmittedByUser.Username,
            a.AdmittedAtUtc, a.DischargedAtUtc, a.DischargeReason,
            a.DischargedByUser?.Username, a.TransferredToBedId);
}
