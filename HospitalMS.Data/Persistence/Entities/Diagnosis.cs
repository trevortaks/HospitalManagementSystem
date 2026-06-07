namespace HospitalMS.Data.Persistence.Entities;

public sealed class Diagnosis
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid EncounterId { get; set; }
    public string IcdCode { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string DiagnosisType { get; set; } = DiagnosisTypes.Primary;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public ClinicalEncounter Encounter { get; set; } = null!;
}

public static class DiagnosisTypes
{
    public const string Primary = "Primary";
    public const string Secondary = "Secondary";
    public const string Differential = "Differential";

    public static readonly string[] All = [Primary, Secondary, Differential];
}
