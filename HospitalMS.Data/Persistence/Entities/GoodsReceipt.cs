namespace HospitalMS.Data.Persistence.Entities;

public sealed class GoodsReceipt
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PurchaseOrderId { get; set; }
    public Guid ReceivedByUserId { get; set; }
    public string? Notes { get; set; }
    public DateTime ReceivedAtUtc { get; set; } = DateTime.UtcNow;

    public PurchaseOrder PurchaseOrder { get; set; } = null!;
    public User ReceivedByUser { get; set; } = null!;
    public ICollection<GoodsReceiptLine> Lines { get; set; } = [];
}

public sealed class GoodsReceiptLine
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid GoodsReceiptId { get; set; }
    public Guid PurchaseOrderLineId { get; set; }
    public int QuantityReceived { get; set; }

    public GoodsReceipt Receipt { get; set; } = null!;
    public PurchaseOrderLine PurchaseOrderLine { get; set; } = null!;
}
