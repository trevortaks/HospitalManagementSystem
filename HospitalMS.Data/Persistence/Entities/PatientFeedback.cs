namespace HospitalMS.Data.Persistence.Entities;

public sealed class PatientFeedback
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PatientId { get; set; }
    public Guid? AppointmentId { get; set; }
    public int OverallRating { get; set; }
    public int? StaffRating { get; set; }
    public int? FacilityRating { get; set; }
    public string? Comments { get; set; }
    public DateTime SubmittedAtUtc { get; set; } = DateTime.UtcNow;

    public Patient Patient { get; set; } = null!;
    public Appointment? Appointment { get; set; }
}
