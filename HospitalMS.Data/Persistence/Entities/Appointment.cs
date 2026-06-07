namespace HospitalMS.Data.Persistence.Entities;

public sealed class Appointment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PatientId { get; set; }
    public Guid DoctorUserId { get; set; }
    public DateTime ScheduledAtUtc { get; set; }
    public int DurationMinutes { get; set; } = 30;
    public string Status { get; set; } = AppointmentStatus.Scheduled;
    public string Type { get; set; } = AppointmentType.General;
    public string? Reason { get; set; }
    public string? Notes { get; set; }
    public string? CancelledReason { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }

    public Patient Patient { get; set; } = null!;
    public User DoctorUser { get; set; } = null!;
}

public static class AppointmentStatus
{
    public const string Scheduled = "Scheduled";
    public const string Confirmed = "Confirmed";
    public const string InProgress = "InProgress";
    public const string Completed = "Completed";
    public const string Cancelled = "Cancelled";
    public const string NoShow = "NoShow";

    public static readonly string[] All = [Scheduled, Confirmed, InProgress, Completed, Cancelled, NoShow];
}

public static class AppointmentType
{
    public const string General = "General";
    public const string FollowUp = "FollowUp";
    public const string Emergency = "Emergency";
    public const string Procedure = "Procedure";
    public const string Teleconsult = "Teleconsult";

    public static readonly string[] All = [General, FollowUp, Emergency, Procedure, Teleconsult];
}
