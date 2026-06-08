namespace HospitalMS.Data.Persistence.Entities;

public sealed class ImagingReport
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid RequestId { get; set; }
    public Guid RadiologyUserId { get; set; }
    public string ReportText { get; set; } = string.Empty;
    public string? Impression { get; set; }
    public DateTime ReportedAtUtc { get; set; } = DateTime.UtcNow;
    public string? AttachmentPath { get; set; }

    public ImagingRequest Request { get; set; } = null!;
    public User RadiologyUser { get; set; } = null!;
}
