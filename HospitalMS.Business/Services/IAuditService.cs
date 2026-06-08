using HospitalMS.Business.Models;

namespace HospitalMS.Business.Services;

public interface IAuditService
{
    Task<AuditEntryResponse> LogAsync(CreateAuditEntryRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AuditEntryResponse>> GetEntriesAsync(AuditEntryFilter filter, int limit = 200, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AuditEntryResponse>> GetEntriesByEntityAsync(string entityType, Guid entityId, CancellationToken cancellationToken = default);
}
