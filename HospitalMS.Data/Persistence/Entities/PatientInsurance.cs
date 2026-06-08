namespace HospitalMS.Data.Persistence.Entities;

public sealed class PatientInsurance
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PatientId { get; set; }
    public Guid ProviderId { get; set; }
    public string PolicyNumber { get; set; } = string.Empty;
    public string? GroupNumber { get; set; }
    public bool IsPrimary { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public Patient Patient { get; set; } = null!;
    public InsuranceProvider Provider { get; set; } = null!;
}
