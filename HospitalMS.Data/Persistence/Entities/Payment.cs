namespace HospitalMS.Data.Persistence.Entities;

public sealed class Payment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid InvoiceId { get; set; }
    public Guid RecordedByUserId { get; set; }
    public decimal Amount { get; set; }
    public string Method { get; set; } = PaymentMethod.Cash;
    public string? ReferenceNumber { get; set; }
    public string? Notes { get; set; }
    public DateTime PaidAtUtc { get; set; } = DateTime.UtcNow;

    public Invoice Invoice { get; set; } = null!;
    public User RecordedByUser { get; set; } = null!;
}

public static class PaymentMethod
{
    public const string Cash         = "Cash";
    public const string Card         = "Card";
    public const string BankTransfer = "BankTransfer";
    public const string Insurance    = "Insurance";
    public const string MobileMoney  = "MobileMoney";
    public static readonly string[] All = [Cash, Card, BankTransfer, Insurance, MobileMoney];
}
