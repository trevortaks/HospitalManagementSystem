namespace HospitalMS.Data.Persistence.Entities;

public sealed class LabOrder
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid EncounterId { get; set; }
    public Guid PatientId { get; set; }
    public Guid OrderedByUserId { get; set; }
    public Guid PanelId { get; set; }
    public string Priority { get; set; } = LabOrderPriority.Routine;
    public string Status { get; set; } = LabOrderStatus.Ordered;
    public DateTime OrderedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? CollectedAtUtc { get; set; }
    public DateTime? ResultedAtUtc { get; set; }
    public string? Notes { get; set; }

    public ClinicalEncounter Encounter { get; set; } = null!;
    public Patient Patient { get; set; } = null!;
    public User OrderedByUser { get; set; } = null!;
    public LabOrderPanel Panel { get; set; } = null!;
    public ICollection<LabResult> Results { get; set; } = [];
}

public static class LabOrderPriority
{
    public const string Routine = "Routine";
    public const string Urgent  = "Urgent";
    public const string Stat    = "Stat";
    public static readonly string[] All = [Routine, Urgent, Stat];
}

public static class LabOrderStatus
{
    public const string Ordered    = "Ordered";
    public const string Collected  = "Collected";
    public const string Processing = "Processing";
    public const string Resulted   = "Resulted";
    public const string Cancelled  = "Cancelled";
    public static readonly string[] All = [Ordered, Collected, Processing, Resulted, Cancelled];
}
