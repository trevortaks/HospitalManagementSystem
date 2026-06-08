namespace HospitalMS.Data.Persistence.Entities;

public sealed class Invoice
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string InvoiceNumber { get; set; } = string.Empty;
    public Guid PatientId { get; set; }
    public Guid? EncounterId { get; set; }
    public Guid CreatedByUserId { get; set; }
    public string Status { get; set; } = InvoiceStatus.Draft;
    public decimal SubtotalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public DateTime? DueDate { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public Patient Patient { get; set; } = null!;
    public ClinicalEncounter? Encounter { get; set; }
    public User CreatedByUser { get; set; } = null!;
    public ICollection<InvoiceLineItem> LineItems { get; set; } = [];
    public ICollection<Payment> Payments { get; set; } = [];
    public ICollection<InsuranceClaim> Claims { get; set; } = [];
}

public static class InvoiceStatus
{
    public const string Draft          = "Draft";
    public const string Issued         = "Issued";
    public const string PartiallyPaid  = "PartiallyPaid";
    public const string Paid           = "Paid";
    public const string Voided         = "Voided";
    public const string Overdue        = "Overdue";
    public static readonly string[] All = [Draft, Issued, PartiallyPaid, Paid, Voided, Overdue];
}
