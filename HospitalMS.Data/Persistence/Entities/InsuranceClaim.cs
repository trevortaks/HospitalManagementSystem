namespace HospitalMS.Data.Persistence.Entities;

public sealed class InsuranceClaim
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid InvoiceId { get; set; }
    public Guid PatientInsuranceId { get; set; }
    public string ClaimNumber { get; set; } = string.Empty;
    public string Status { get; set; } = ClaimStatus.Pending;
    public decimal? ApprovedAmount { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime SubmittedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ResolvedAtUtc { get; set; }

    public Invoice Invoice { get; set; } = null!;
    public PatientInsurance PatientInsurance { get; set; } = null!;
}

public static class ClaimStatus
{
    public const string Pending   = "Pending";
    public const string Submitted = "Submitted";
    public const string Approved  = "Approved";
    public const string Rejected  = "Rejected";
    public const string Paid      = "Paid";
    public static readonly string[] All = [Pending, Submitted, Approved, Rejected, Paid];
}
