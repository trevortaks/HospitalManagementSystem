namespace HospitalMS.Data.Persistence.Entities;

public sealed class BedAllocation
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid BedId { get; set; }
    public Guid PatientId { get; set; }
    public Guid? EncounterId { get; set; }
    public Guid AdmittedByUserId { get; set; }
    public DateTime AdmittedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? DischargedAtUtc { get; set; }
    public string? DischargeReason { get; set; }
    public Guid? DischargedByUserId { get; set; }
    public Guid? TransferredToBedId { get; set; }

    public Bed Bed { get; set; } = null!;
    public Patient Patient { get; set; } = null!;
    public ClinicalEncounter? Encounter { get; set; }
    public User AdmittedByUser { get; set; } = null!;
    public User? DischargedByUser { get; set; }
    public Bed? TransferredToBed { get; set; }
}
