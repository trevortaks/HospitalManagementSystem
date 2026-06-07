namespace HospitalMS.Data.Persistence.Entities;

public sealed class Prescription
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid EncounterId { get; set; }
    public Guid PatientId { get; set; }
    public Guid PrescribedByUserId { get; set; }
    public Guid MedicationId { get; set; }
    public string Dose { get; set; } = string.Empty;
    public string Frequency { get; set; } = string.Empty;
    public int? DurationDays { get; set; }
    public int? QuantityDispensed { get; set; }
    public string? Instructions { get; set; }
    public string Status { get; set; } = PrescriptionStatus.Active;
    public DateTime PrescribedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? DispensedAtUtc { get; set; }
    public Guid? DispensedByUserId { get; set; }

    public ClinicalEncounter Encounter { get; set; } = null!;
    public Patient Patient { get; set; } = null!;
    public User PrescribedByUser { get; set; } = null!;
    public Medication Medication { get; set; } = null!;
    public User? DispensedByUser { get; set; }
}

public static class PrescriptionStatus
{
    public const string Active = "Active";
    public const string Dispensed = "Dispensed";
    public const string Cancelled = "Cancelled";
    public const string Expired = "Expired";

    public static readonly string[] All = [Active, Dispensed, Cancelled, Expired];
}
