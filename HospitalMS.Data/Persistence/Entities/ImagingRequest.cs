namespace HospitalMS.Data.Persistence.Entities;

public sealed class ImagingRequest
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid EncounterId { get; set; }
    public Guid PatientId { get; set; }
    public Guid RequestedByUserId { get; set; }
    public string Modality { get; set; } = string.Empty;
    public string? BodyPart { get; set; }
    public string? ClinicalIndication { get; set; }
    public string Priority { get; set; } = LabOrderPriority.Routine;
    public string Status { get; set; } = ImagingRequestStatus.Requested;
    public DateTime RequestedAtUtc { get; set; } = DateTime.UtcNow;

    public ClinicalEncounter Encounter { get; set; } = null!;
    public Patient Patient { get; set; } = null!;
    public User RequestedByUser { get; set; } = null!;
    public ImagingReport? Report { get; set; }
}

public static class ImagingModality
{
    public const string XRay      = "XRay";
    public const string CT        = "CT";
    public const string MRI       = "MRI";
    public const string Ultrasound = "Ultrasound";
    public const string PET       = "PET";
    public static readonly string[] All = [XRay, CT, MRI, Ultrasound, PET];
}

public static class ImagingRequestStatus
{
    public const string Requested  = "Requested";
    public const string Scheduled  = "Scheduled";
    public const string Acquired   = "Acquired";
    public const string Reported   = "Reported";
    public const string Cancelled  = "Cancelled";
    public static readonly string[] All = [Requested, Scheduled, Acquired, Reported, Cancelled];
}
