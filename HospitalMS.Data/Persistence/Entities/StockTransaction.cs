namespace HospitalMS.Data.Persistence.Entities;

public sealed class StockTransaction
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ItemId { get; set; }
    public string Type { get; set; } = StockTransactionType.In;
    public int Quantity { get; set; }
    public int StockAfter { get; set; }
    public string? ReferenceType { get; set; }
    public Guid? ReferenceId { get; set; }
    public string? Notes { get; set; }
    public Guid PerformedByUserId { get; set; }
    public DateTime TransactedAtUtc { get; set; } = DateTime.UtcNow;

    public InventoryItem Item { get; set; } = null!;
    public User PerformedByUser { get; set; } = null!;
}

public static class StockTransactionType
{
    public const string In         = "In";
    public const string Out        = "Out";
    public const string Adjustment = "Adjustment";
    public const string Waste      = "Waste";
    public const string Return     = "Return";
}
