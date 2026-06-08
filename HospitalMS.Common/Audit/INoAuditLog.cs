namespace HospitalMS.Common.Audit;

/// <summary>
/// Marker interface — entities implementing this are excluded from the AuditInterceptor
/// to prevent recursive audit-of-audit-writes.
/// </summary>
public interface INoAuditLog;
