namespace HospitalMS.Data.Persistence.Entities;

public sealed class InventoryItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CategoryId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Unit { get; set; } = "Unit";
    public int ReorderLevel { get; set; }
    public int CurrentStock { get; set; }
    public decimal UnitCost { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public InventoryCategory Category { get; set; } = null!;
    public ICollection<StockTransaction> Transactions { get; set; } = [];
}
