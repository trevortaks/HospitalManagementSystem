using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace HospitalMS.Common.Audit;

public sealed class AuditInterceptor(IAuditLogger auditLogger) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        LogAuditEntriesAsync(eventData.Context, CancellationToken.None).GetAwaiter().GetResult();
        return base.SavingChanges(eventData, result);
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        await LogAuditEntriesAsync(eventData.Context, cancellationToken);
        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private async Task LogAuditEntriesAsync(DbContext? context, CancellationToken cancellationToken)
    {
        if (context is null)
        {
            return;
        }

        var auditEntries = context.ChangeTracker
            .Entries()
            .Where(static entry => entry.Entity is not AuditLog)
            .Where(static entry => entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .Select(CreateAuditLog)
            .ToArray();

        foreach (var auditEntry in auditEntries)
        {
            await auditLogger.LogAsync(auditEntry, cancellationToken);
        }
    }

    private static AuditLog CreateAuditLog(EntityEntry entry)
    {
        var details = JsonSerializer.Serialize(new
        {
            Entity = entry.Metadata.ClrType.Name,
            State = entry.State.ToString(),
            Keys = GetPrimaryKeys(entry),
            Changes = GetChangedValues(entry)
        });

        return new AuditLog
        {
            Action = entry.State.ToString(),
            Resource = entry.Metadata.ClrType.Name,
            Details = details,
            Timestamp = DateTimeOffset.UtcNow
        };
    }

    private static IDictionary<string, object?> GetPrimaryKeys(EntityEntry entry)
    {
        return entry.Properties
            .Where(static property => property.Metadata.IsPrimaryKey())
            .ToDictionary(property => property.Metadata.Name, property => entry.State == EntityState.Added ? property.CurrentValue : property.OriginalValue);
    }

    private static IDictionary<string, object?> GetChangedValues(EntityEntry entry)
    {
        return entry.Properties
            .Where(property => entry.State != EntityState.Modified || property.IsModified)
            .ToDictionary(
                property => property.Metadata.Name,
                property => entry.State switch
                {
                    EntityState.Added => property.CurrentValue,
                    EntityState.Deleted => property.OriginalValue,
                    _ => new { Original = property.OriginalValue, Current = property.CurrentValue }
                });
    }
}
