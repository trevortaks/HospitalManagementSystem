namespace HospitalMS.Data.Persistence.Entities;

public sealed class InvoiceLineItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid InvoiceId { get; set; }
    public Guid ChargeItemId { get; set; }
    public string Description { get; set; } = string.Empty;
    public int Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }

    public Invoice Invoice { get; set; } = null!;
    public ChargeItem ChargeItem { get; set; } = null!;
}
