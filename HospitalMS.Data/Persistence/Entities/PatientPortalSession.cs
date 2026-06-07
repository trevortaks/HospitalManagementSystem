namespace HospitalMS.Data.Persistence.Entities;

public sealed class PatientPortalSession
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public Guid PatientId { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime LoginAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime LastActivityAtUtc { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
    public Patient Patient { get; set; } = null!;
}
