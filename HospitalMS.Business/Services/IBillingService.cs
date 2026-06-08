using HospitalMS.Business.Models;

namespace HospitalMS.Business.Services;

public interface IBillingService
{
    // Charge items
    Task<IReadOnlyList<ChargeItemResponse>> GetAllChargeItemsAsync(bool? activeOnly = null, CancellationToken cancellationToken = default);
    Task<ChargeItemResponse?> GetChargeItemByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ChargeItemResponse> CreateChargeItemAsync(CreateChargeItemRequest request, CancellationToken cancellationToken = default);
    Task<ChargeItemResponse> UpdateChargeItemAsync(Guid id, UpdateChargeItemRequest request, CancellationToken cancellationToken = default);
    Task<ChargeItemResponse> ToggleChargeItemActiveAsync(Guid id, CancellationToken cancellationToken = default);

    // Invoices
    Task<IReadOnlyList<InvoiceResponse>> GetAllInvoicesAsync(Guid? patientId = null, string? status = null, CancellationToken cancellationToken = default);
    Task<InvoiceResponse?> GetInvoiceByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<InvoiceResponse> CreateInvoiceAsync(CreateInvoiceRequest request, CancellationToken cancellationToken = default);
    Task<InvoiceResponse> UpdateInvoiceAsync(Guid id, UpdateInvoiceRequest request, CancellationToken cancellationToken = default);
    Task<InvoiceResponse> AddLineItemAsync(Guid invoiceId, AddLineItemRequest request, CancellationToken cancellationToken = default);
    Task<InvoiceResponse> RemoveLineItemAsync(Guid invoiceId, Guid lineItemId, CancellationToken cancellationToken = default);
    Task<InvoiceResponse> IssueInvoiceAsync(Guid id, CancellationToken cancellationToken = default);
    Task<InvoiceResponse> VoidInvoiceAsync(Guid id, CancellationToken cancellationToken = default);

    // Payments
    Task<InvoiceResponse> RecordPaymentAsync(RecordPaymentRequest request, CancellationToken cancellationToken = default);
}
