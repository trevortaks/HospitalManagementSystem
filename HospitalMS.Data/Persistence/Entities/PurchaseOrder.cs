namespace HospitalMS.Data.Persistence.Entities;

public sealed class PurchaseOrder
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SupplierId { get; set; }
    public Guid OrderedByUserId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string Status { get; set; } = PurchaseOrderStatus.Draft;
    public string? Notes { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }
    public DateTime OrderedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public Supplier Supplier { get; set; } = null!;
    public User OrderedByUser { get; set; } = null!;
    public ICollection<PurchaseOrderLine> Lines { get; set; } = [];
    public ICollection<GoodsReceipt> Receipts { get; set; } = [];
}

public static class PurchaseOrderStatus
{
    public const string Draft             = "Draft";
    public const string Submitted        = "Submitted";
    public const string Approved         = "Approved";
    public const string PartiallyReceived = "PartiallyReceived";
    public const string Received         = "Received";
    public const string Cancelled        = "Cancelled";
}
