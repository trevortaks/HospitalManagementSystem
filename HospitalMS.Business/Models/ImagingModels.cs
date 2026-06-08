namespace HospitalMS.Business.Models;

public sealed record CreateImagingRequestRequest(
    Guid EncounterId,
    Guid PatientId,
    Guid RequestedByUserId,
    string Modality,
    string? BodyPart = null,
    string? ClinicalIndication = null,
    string Priority = "Routine");

public sealed record ImagingRequestResponse(
    Guid Id,
    Guid EncounterId,
    Guid PatientId,
    string PatientName,
    string PatientMrn,
    Guid RequestedByUserId,
    string RequestedByName,
    string Modality,
    string? BodyPart,
    string? ClinicalIndication,
    string Priority,
    string Status,
    DateTime RequestedAtUtc,
    ImagingReportResponse? Report);

public sealed record CreateImagingReportRequest(
    Guid RadiologyUserId,
    string ReportText,
    string? Impression = null,
    string? AttachmentPath = null);

public sealed record ImagingReportResponse(
    Guid Id,
    Guid RequestId,
    Guid RadiologyUserId,
    string RadiologyUserName,
    string ReportText,
    string? Impression,
    DateTime ReportedAtUtc,
    string? AttachmentPath);
