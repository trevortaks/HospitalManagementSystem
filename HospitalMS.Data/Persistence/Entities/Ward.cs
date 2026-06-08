namespace HospitalMS.Data.Persistence.Entities;

public sealed class Ward
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string WardType { get; set; } = "General";
    public int TotalBeds { get; set; }
    public int FloorNumber { get; set; } = 1;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<Bed> Beds { get; set; } = [];
}

public static class WardType
{
    public const string General    = "General";
    public const string ICU        = "ICU";
    public const string HDU        = "HDU";
    public const string Maternity  = "Maternity";
    public const string Paediatric = "Paediatric";
    public const string Emergency  = "Emergency";
}
