namespace HospitalMS.Data.Persistence.Entities;

public sealed class Medication
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string GenericName { get; set; } = string.Empty;
    public string? BrandName { get; set; }
    public string Form { get; set; } = string.Empty;
    public string? Strength { get; set; }
    public string? RouteOfAdministration { get; set; }
    public bool IsControlled { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}

public static class MedicationForm
{
    public const string Tablet = "Tablet";
    public const string Capsule = "Capsule";
    public const string Liquid = "Liquid";
    public const string Injection = "Injection";
    public const string Topical = "Topical";
    public const string Inhaler = "Inhaler";
    public const string Patch = "Patch";
    public const string Drops = "Drops";
    public const string Suppository = "Suppository";

    public static readonly string[] All =
        [Tablet, Capsule, Liquid, Injection, Topical, Inhaler, Patch, Drops, Suppository];
}

public static class MedicationRoute
{
    public const string Oral = "Oral";
    public const string Intravenous = "Intravenous";
    public const string Intramuscular = "Intramuscular";
    public const string Subcutaneous = "Subcutaneous";
    public const string Topical = "Topical";
    public const string Inhaled = "Inhaled";
    public const string Rectal = "Rectal";
    public const string Sublingual = "Sublingual";

    public static readonly string[] All =
        [Oral, Intravenous, Intramuscular, Subcutaneous, Topical, Inhaled, Rectal, Sublingual];
}
