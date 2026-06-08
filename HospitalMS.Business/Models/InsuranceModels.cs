namespace HospitalMS.Business.Models;

public sealed record InsuranceProviderResponse(
    Guid Id,
    string Name,
    string? ContactPhone,
    string? ContactEmail,
    string? Address,
    bool IsActive,
    DateTime CreatedAtUtc);

public sealed record CreateInsuranceProviderRequest(
    string Name,
    string? ContactPhone = null,
    string? ContactEmail = null,
    string? Address = null);

public sealed record PatientInsuranceResponse(
    Guid Id,
    Guid PatientId,
    Guid ProviderId,
    string ProviderName,
    string PolicyNumber,
    string? GroupNumber,
    bool IsPrimary,
    DateTime? ExpiresAt,
    bool IsActive,
    DateTime CreatedAtUtc);

public sealed record AddPatientInsuranceRequest(
    Guid PatientId,
    Guid ProviderId,
    string PolicyNumber,
    string? GroupNumber = null,
    bool IsPrimary = false,
    DateTime? ExpiresAt = null);

public sealed record InsuranceClaimResponse(
    Guid Id,
    Guid InvoiceId,
    string InvoiceNumber,
    Guid PatientInsuranceId,
    string ProviderName,
    string PolicyNumber,
    string ClaimNumber,
    string Status,
    decimal? ApprovedAmount,
    string? RejectionReason,
    DateTime SubmittedAtUtc,
    DateTime? ResolvedAtUtc);

public sealed record CreateInsuranceClaimRequest(
    Guid InvoiceId,
    Guid PatientInsuranceId,
    string ClaimNumber);

public sealed record UpdateClaimStatusRequest(
    string Status,
    decimal? ApprovedAmount = null,
    string? RejectionReason = null);
