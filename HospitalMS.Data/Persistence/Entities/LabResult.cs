namespace HospitalMS.Data.Persistence.Entities;

public sealed class LabResult
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrderId { get; set; }
    public Guid RecordedByUserId { get; set; }
    public string AnalyteName { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? Unit { get; set; }
    public string? ReferenceRange { get; set; }
    public string Flag { get; set; } = LabResultFlag.Normal;
    public DateTime RecordedAtUtc { get; set; } = DateTime.UtcNow;

    public LabOrder Order { get; set; } = null!;
    public User RecordedByUser { get; set; } = null!;
}

public static class LabResultFlag
{
    public const string Normal = "Normal";
    public const string High   = "H";
    public const string Low    = "L";
    public const string CriticalHigh = "HH";
    public const string CriticalLow  = "LL";
    public static readonly string[] All = [Normal, High, Low, CriticalHigh, CriticalLow];
}
