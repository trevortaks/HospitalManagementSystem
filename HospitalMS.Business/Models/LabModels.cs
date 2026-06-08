namespace HospitalMS.Business.Models;

// Lab Order Panels
public sealed record CreateLabOrderPanelRequest(
    string Code,
    string Name,
    string? Category = null,
    bool IsActive = true);

public sealed record LabOrderPanelResponse(
    Guid Id,
    string Code,
    string Name,
    string? Category,
    bool IsActive,
    DateTime CreatedAtUtc);

// Lab Orders
public sealed record CreateLabOrderRequest(
    Guid EncounterId,
    Guid PatientId,
    Guid OrderedByUserId,
    Guid PanelId,
    string Priority = "Routine",
    string? Notes = null);

public sealed record CollectLabOrderRequest(DateTime? CollectedAtUtc = null);

public sealed record LabOrderResponse(
    Guid Id,
    Guid EncounterId,
    Guid PatientId,
    string PatientName,
    string PatientMrn,
    Guid OrderedByUserId,
    string OrderedByName,
    Guid PanelId,
    string PanelCode,
    string PanelName,
    string? PanelCategory,
    string Priority,
    string Status,
    DateTime OrderedAtUtc,
    DateTime? CollectedAtUtc,
    DateTime? ResultedAtUtc,
    string? Notes,
    IReadOnlyList<LabResultResponse> Results);

// Lab Results
public sealed record AddLabResultRequest(
    string AnalyteName,
    string Value,
    Guid RecordedByUserId,
    string? Unit = null,
    string? ReferenceRange = null,
    string Flag = "Normal");

public sealed record LabResultResponse(
    Guid Id,
    Guid OrderId,
    string AnalyteName,
    string Value,
    string? Unit,
    string? ReferenceRange,
    string Flag,
    string RecordedByName,
    DateTime RecordedAtUtc);
