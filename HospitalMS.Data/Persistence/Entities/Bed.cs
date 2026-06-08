namespace HospitalMS.Data.Persistence.Entities;

public sealed class Bed
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid WardId { get; set; }
    public string BedNumber { get; set; } = string.Empty;
    public string BedType { get; set; } = "Standard";
    public string Status { get; set; } = "Available";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public Ward Ward { get; set; } = null!;
    public ICollection<BedAllocation> Allocations { get; set; } = [];
}

public static class BedType
{
    public const string Standard  = "Standard";
    public const string Isolation = "Isolation";
    public const string Monitored = "Monitored";
    public const string ICU       = "ICU";
}

public static class BedStatus
{
    public const string Available    = "Available";
    public const string Occupied     = "Occupied";
    public const string Reserved     = "Reserved";
    public const string OutOfService = "OutOfService";
    public const string Cleaning     = "Cleaning";
}
