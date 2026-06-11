namespace HospitalMS.Data.Persistence.Entities;

public sealed class AppointmentVitals
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid AppointmentId { get; set; }
    public Guid RecordedByUserId { get; set; }
    public DateTime RecordedAtUtc { get; set; } = DateTime.UtcNow;
    public decimal? HeightCm { get; set; }
    public decimal? WeightKg { get; set; }
    public decimal? TemperatureCelsius { get; set; }
    public int? BloodPressureSystolic { get; set; }
    public int? BloodPressureDiastolic { get; set; }
    public int? HeartRateBpm { get; set; }
    public int? RespiratoryRate { get; set; }
    public decimal? OxygenSaturationPct { get; set; }
    public string? Notes { get; set; }

    public Appointment Appointment { get; set; } = null!;
    public User RecordedByUser { get; set; } = null!;
}
