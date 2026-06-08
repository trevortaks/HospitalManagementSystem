namespace HospitalMS.Business.Models;

public sealed record ChargeItemResponse(
    Guid Id,
    string Code,
    string Description,
    string? Category,
    decimal UnitPrice,
    bool IsActive,
    DateTime CreatedAtUtc);

public sealed record CreateChargeItemRequest(
    string Code,
    string Description,
    string? Category,
    decimal UnitPrice,
    bool IsActive = true);

public sealed record UpdateChargeItemRequest(
    string Description,
    string? Category,
    decimal UnitPrice);

public sealed record InvoiceLineItemResponse(
    Guid Id,
    Guid ChargeItemId,
    string ChargeItemCode,
    string Description,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice);

public sealed record AddLineItemRequest(
    Guid ChargeItemId,
    int Quantity = 1);

public sealed record PaymentResponse(
    Guid Id,
    Guid InvoiceId,
    decimal Amount,
    string Method,
    string? ReferenceNumber,
    string? Notes,
    string RecordedByName,
    DateTime PaidAtUtc);

public sealed record RecordPaymentRequest(
    Guid InvoiceId,
    decimal Amount,
    string Method,
    Guid RecordedByUserId,
    string? ReferenceNumber = null,
    string? Notes = null);

public sealed record InvoiceResponse(
    Guid Id,
    string InvoiceNumber,
    Guid PatientId,
    string PatientName,
    string PatientMrn,
    Guid? EncounterId,
    string Status,
    decimal SubtotalAmount,
    decimal DiscountAmount,
    decimal TaxAmount,
    decimal TotalAmount,
    decimal PaidAmount,
    decimal BalanceDue,
    DateTime? DueDate,
    string? Notes,
    string CreatedByName,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc,
    IReadOnlyList<InvoiceLineItemResponse> LineItems,
    IReadOnlyList<PaymentResponse> Payments);

public sealed record CreateInvoiceRequest(
    Guid PatientId,
    Guid CreatedByUserId,
    Guid? EncounterId = null,
    DateTime? DueDate = null,
    string? Notes = null);

public sealed record UpdateInvoiceRequest(
    decimal DiscountAmount,
    decimal TaxAmount,
    DateTime? DueDate,
    string? Notes);
