namespace HospitalMS.Data.Persistence.Entities;

public sealed class ClinicalEncounter
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PatientId { get; set; }
    public Guid? AppointmentId { get; set; }
    public Guid AttendingDoctorId { get; set; }
    public string EncounterType { get; set; } = string.Empty;
    public DateTime StartedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? EndedAtUtc { get; set; }
    public string? ChiefComplaint { get; set; }
    public string? HistoryOfPresentIllness { get; set; }
    public string? Examination { get; set; }
    public string? Assessment { get; set; }
    public string? Plan { get; set; }
    public string? FollowUpNotes { get; set; }
    public bool IsClosed { get; set; } = false;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }

    public Patient Patient { get; set; } = null!;
    public Appointment? Appointment { get; set; }
    public User AttendingDoctor { get; set; } = null!;
    public ICollection<Diagnosis> Diagnoses { get; set; } = [];
    public ICollection<VitalSigns> VitalSigns { get; set; } = [];
}

public static class EncounterType
{
    public const string Outpatient = "Outpatient";
    public const string Inpatient = "Inpatient";
    public const string Emergency = "Emergency";
    public const string Teleconsult = "Teleconsult";

    public static readonly string[] All = [Outpatient, Inpatient, Emergency, Teleconsult];
}
