using HospitalMS.Business.Models;
using HospitalMS.Data.Persistence;
using HospitalMS.Data.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace HospitalMS.Business.Services;

public sealed class AuditService(HospitalDbContext dbContext) : IAuditService
{
    public async Task<AuditEntryResponse> LogAsync(CreateAuditEntryRequest request, CancellationToken cancellationToken = default)
    {
        var entry = new AuditEntry
        {
            EntityType           = request.EntityType,
            EntityId             = request.EntityId,
            Action               = request.Action,
            Details              = request.Details,
            PerformedByUserId    = request.PerformedByUserId,
            PerformedByUsername  = request.PerformedByUsername,
            IpAddress            = request.IpAddress
        };
        dbContext.AuditEntries.Add(entry);
        await dbContext.SaveChangesAsync(cancellationToken);
        return ToResponse(entry);
    }

    public async Task<IReadOnlyList<AuditEntryResponse>> GetEntriesAsync(
        AuditEntryFilter filter, int limit = 200, CancellationToken cancellationToken = default)
    {
        var query = dbContext.AuditEntries.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.EntityType))
            query = query.Where(a => a.EntityType == filter.EntityType);

        if (filter.EntityId.HasValue)
            query = query.Where(a => a.EntityId == filter.EntityId);

        if (!string.IsNullOrWhiteSpace(filter.Action))
            query = query.Where(a => a.Action == filter.Action);

        if (filter.PerformedByUserId.HasValue)
            query = query.Where(a => a.PerformedByUserId == filter.PerformedByUserId);

        if (filter.From.HasValue)
            query = query.Where(a => a.PerformedAtUtc >= filter.From.Value);

        if (filter.To.HasValue)
            query = query.Where(a => a.PerformedAtUtc <= filter.To.Value);

        var entries = await query
            .OrderByDescending(a => a.PerformedAtUtc)
            .Take(limit)
            .ToListAsync(cancellationToken);

        return entries.Select(ToResponse).ToArray();
    }

    public async Task<IReadOnlyList<AuditEntryResponse>> GetEntriesByEntityAsync(
        string entityType, Guid entityId, CancellationToken cancellationToken = default)
    {
        var entries = await dbContext.AuditEntries.AsNoTracking()
            .Where(a => a.EntityType == entityType && a.EntityId == entityId)
            .OrderByDescending(a => a.PerformedAtUtc)
            .ToListAsync(cancellationToken);

        return entries.Select(ToResponse).ToArray();
    }

    private static AuditEntryResponse ToResponse(AuditEntry a) =>
        new(a.Id, a.EntityType, a.EntityId, a.Action, a.Details,
            a.PerformedByUserId, a.PerformedByUsername, a.IpAddress, a.PerformedAtUtc);
}
