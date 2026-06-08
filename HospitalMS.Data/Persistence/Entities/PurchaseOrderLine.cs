namespace HospitalMS.Data.Persistence.Entities;

public sealed class PurchaseOrderLine
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PurchaseOrderId { get; set; }
    public Guid InventoryItemId { get; set; }
    public string Description { get; set; } = string.Empty;
    public int QuantityOrdered { get; set; }
    public int QuantityReceived { get; set; }
    public decimal UnitCost { get; set; }
    public decimal TotalCost { get; set; }

    public PurchaseOrder PurchaseOrder { get; set; } = null!;
    public InventoryItem InventoryItem { get; set; } = null!;
}
