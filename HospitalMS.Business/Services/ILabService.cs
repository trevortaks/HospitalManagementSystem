using HospitalMS.Business.Models;

namespace HospitalMS.Business.Services;

public interface ILabService
{
    // Panels
    Task<IReadOnlyList<LabOrderPanelResponse>> GetAllPanelsAsync(bool? activeOnly = null, CancellationToken cancellationToken = default);
    Task<LabOrderPanelResponse?> GetPanelByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<LabOrderPanelResponse> CreatePanelAsync(CreateLabOrderPanelRequest request, CancellationToken cancellationToken = default);
    Task<LabOrderPanelResponse> TogglePanelActiveAsync(Guid id, CancellationToken cancellationToken = default);

    // Orders
    Task<IReadOnlyList<LabOrderResponse>> GetAllOrdersAsync(Guid? patientId = null, Guid? encounterId = null, string? status = null, CancellationToken cancellationToken = default);
    Task<LabOrderResponse?> GetOrderByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<LabOrderResponse> CreateOrderAsync(CreateLabOrderRequest request, CancellationToken cancellationToken = default);
    Task<LabOrderResponse> CollectOrderAsync(Guid id, CancellationToken cancellationToken = default);
    Task<LabOrderResponse> CancelOrderAsync(Guid id, CancellationToken cancellationToken = default);

    // Results
    Task<LabOrderResponse> AddResultAsync(Guid orderId, AddLabResultRequest request, CancellationToken cancellationToken = default);
}
