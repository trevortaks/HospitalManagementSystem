using HospitalMS.Business.Models;
using HospitalMS.Data.Persistence;
using HospitalMS.Data.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace HospitalMS.Business.Services;

public sealed class LabService(HospitalDbContext dbContext) : ILabService
{
    // ── Panels ──────────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<LabOrderPanelResponse>> GetAllPanelsAsync(bool? activeOnly = null, CancellationToken cancellationToken = default)
    {
        var query = dbContext.LabOrderPanels.AsNoTracking().AsQueryable();
        if (activeOnly == true) query = query.Where(p => p.IsActive);
        var panels = await query.OrderBy(p => p.Name).ToListAsync(cancellationToken);
        return panels.Select(ToPanelResponse).ToArray();
    }

    public async Task<LabOrderPanelResponse?> GetPanelByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var panel = await dbContext.LabOrderPanels.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        return panel is null ? null : ToPanelResponse(panel);
    }

    public async Task<LabOrderPanelResponse> CreatePanelAsync(CreateLabOrderPanelRequest request, CancellationToken cancellationToken = default)
    {
        var panel = new LabOrderPanel
        {
            Code = request.Code,
            Name = request.Name,
            Category = request.Category,
            IsActive = request.IsActive
        };
        dbContext.LabOrderPanels.Add(panel);
        await dbContext.SaveChangesAsync(cancellationToken);
        return ToPanelResponse(panel);
    }

    public async Task<LabOrderPanelResponse> TogglePanelActiveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var panel = await dbContext.LabOrderPanels.FindAsync([id], cancellationToken)
            ?? throw new KeyNotFoundException($"Lab panel '{id}' not found.");
        panel.IsActive = !panel.IsActive;
        await dbContext.SaveChangesAsync(cancellationToken);
        return ToPanelResponse(panel);
    }

    // ── Orders ───────────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<LabOrderResponse>> GetAllOrdersAsync(
        Guid? patientId = null, Guid? encounterId = null, string? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.LabOrders
            .AsNoTracking()
            .Include(o => o.Patient)
            .Include(o => o.OrderedByUser)
            .Include(o => o.Panel)
            .Include(o => o.Results).ThenInclude(r => r.RecordedByUser)
            .AsQueryable();

        if (patientId.HasValue)   query = query.Where(o => o.PatientId == patientId.Value);
        if (encounterId.HasValue) query = query.Where(o => o.EncounterId == encounterId.Value);
        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(o => o.Status == status);

        var orders = await query.OrderByDescending(o => o.OrderedAtUtc).ToListAsync(cancellationToken);
        return orders.Select(ToOrderResponse).ToArray();
    }

    public async Task<LabOrderResponse?> GetOrderByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var order = await LoadOrderAsync(id, cancellationToken);
        return order is null ? null : ToOrderResponse(order);
    }

    public async Task<LabOrderResponse> CreateOrderAsync(CreateLabOrderRequest request, CancellationToken cancellationToken = default)
    {
        var order = new LabOrder
        {
            EncounterId = request.EncounterId,
            PatientId = request.PatientId,
            OrderedByUserId = request.OrderedByUserId,
            PanelId = request.PanelId,
            Priority = request.Priority,
            Notes = request.Notes
        };
        dbContext.LabOrders.Add(order);
        await dbContext.SaveChangesAsync(cancellationToken);
        return ToOrderResponse(await LoadOrderAsync(order.Id, cancellationToken)!);
    }

    public async Task<LabOrderResponse> CollectOrderAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var order = await dbContext.LabOrders.FindAsync([id], cancellationToken)
            ?? throw new KeyNotFoundException($"Lab order '{id}' not found.");

        if (order.Status == LabOrderStatus.Cancelled)
            throw new InvalidOperationException("Cannot collect a cancelled order.");

        order.Status = LabOrderStatus.Collected;
        order.CollectedAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        return ToOrderResponse(await LoadOrderAsync(id, cancellationToken)!);
    }

    public async Task<LabOrderResponse> CancelOrderAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var order = await dbContext.LabOrders.FindAsync([id], cancellationToken)
            ?? throw new KeyNotFoundException($"Lab order '{id}' not found.");

        if (order.Status == LabOrderStatus.Resulted)
            throw new InvalidOperationException("Cannot cancel an order that has already been resulted.");

        order.Status = LabOrderStatus.Cancelled;
        await dbContext.SaveChangesAsync(cancellationToken);
        return ToOrderResponse(await LoadOrderAsync(id, cancellationToken)!);
    }

    // ── Results ──────────────────────────────────────────────────────────────

    public async Task<LabOrderResponse> AddResultAsync(Guid orderId, AddLabResultRequest request, CancellationToken cancellationToken = default)
    {
        var order = await dbContext.LabOrders.FindAsync([orderId], cancellationToken)
            ?? throw new KeyNotFoundException($"Lab order '{orderId}' not found.");

        if (order.Status == LabOrderStatus.Cancelled)
            throw new InvalidOperationException("Cannot add results to a cancelled order.");

        var result = new LabResult
        {
            OrderId = orderId,
            RecordedByUserId = request.RecordedByUserId,
            AnalyteName = request.AnalyteName,
            Value = request.Value,
            Unit = request.Unit,
            ReferenceRange = request.ReferenceRange,
            Flag = request.Flag
        };
        dbContext.LabResults.Add(result);

        order.Status = LabOrderStatus.Resulted;
        order.ResultedAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        return ToOrderResponse(await LoadOrderAsync(orderId, cancellationToken)!);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private async Task<LabOrder?> LoadOrderAsync(Guid id, CancellationToken cancellationToken) =>
        await dbContext.LabOrders
            .AsNoTracking()
            .Include(o => o.Patient)
            .Include(o => o.OrderedByUser)
            .Include(o => o.Panel)
            .Include(o => o.Results).ThenInclude(r => r.RecordedByUser)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

    private static LabOrderPanelResponse ToPanelResponse(LabOrderPanel p) =>
        new(p.Id, p.Code, p.Name, p.Category, p.IsActive, p.CreatedAtUtc);

    private static LabOrderResponse ToOrderResponse(LabOrder o) => new(
        o.Id, o.EncounterId,
        o.PatientId, $"{o.Patient.FirstName} {o.Patient.LastName}", o.Patient.MedicalRecordNumber,
        o.OrderedByUserId, o.OrderedByUser.Username,
        o.PanelId, o.Panel.Code, o.Panel.Name, o.Panel.Category,
        o.Priority, o.Status,
        o.OrderedAtUtc, o.CollectedAtUtc, o.ResultedAtUtc, o.Notes,
        o.Results.Select(r => new LabResultResponse(
            r.Id, r.OrderId, r.AnalyteName, r.Value, r.Unit, r.ReferenceRange,
            r.Flag, r.RecordedByUser.Username, r.RecordedAtUtc)).ToArray());
}
