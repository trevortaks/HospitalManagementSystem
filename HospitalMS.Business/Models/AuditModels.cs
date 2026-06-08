namespace HospitalMS.Business.Models;

public record AuditEntryResponse(
    Guid Id,
    string EntityType,
    Guid? EntityId,
    string Action,
    string? Details,
    Guid? PerformedByUserId,
    string? PerformedByUsername,
    string? IpAddress,
    DateTime PerformedAtUtc);

public record CreateAuditEntryRequest(
    string EntityType,
    string Action,
    Guid? EntityId = null,
    string? Details = null,
    Guid? PerformedByUserId = null,
    string? PerformedByUsername = null,
    string? IpAddress = null);

public record AuditEntryFilter(
    string? EntityType = null,
    Guid? EntityId = null,
    string? Action = null,
    Guid? PerformedByUserId = null,
    DateTime? From = null,
    DateTime? To = null);
