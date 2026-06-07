using HospitalMS.Data.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace HospitalMS.Data.Persistence;

public class HospitalDbContext(DbContextOptions<HospitalDbContext> options) : DbContext(options)
{
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<ClinicalEncounter> ClinicalEncounters => Set<ClinicalEncounter>();
    public DbSet<Diagnosis> Diagnoses => Set<Diagnosis>();
    public DbSet<VitalSigns> VitalSigns => Set<VitalSigns>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Patient>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.MedicalRecordNumber).HasMaxLength(32).IsRequired();
            entity.Property(p => p.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(p => p.LastName).HasMaxLength(100).IsRequired();
            entity.Property(p => p.Email).HasMaxLength(256);
            entity.Property(p => p.PhoneNumber).HasMaxLength(20);
            entity.Property(p => p.Gender).HasMaxLength(20);
            entity.Property(p => p.BloodGroup).HasMaxLength(5);
            entity.Property(p => p.AddressLine1).HasMaxLength(200);
            entity.Property(p => p.City).HasMaxLength(100);
            entity.Property(p => p.PostalCode).HasMaxLength(20);
            entity.Property(p => p.Country).HasMaxLength(100);
            entity.Property(p => p.EmergencyContactName).HasMaxLength(200);
            entity.Property(p => p.EmergencyContactPhone).HasMaxLength(20);
            entity.HasIndex(p => p.MedicalRecordNumber).IsUnique();
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Username).HasMaxLength(100).IsRequired();
            entity.Property(u => u.Email).HasMaxLength(256).IsRequired();
            entity.Property(u => u.Role).HasMaxLength(100).IsRequired();
            entity.Property(u => u.PasswordHash).HasMaxLength(100).IsRequired();
            entity.HasIndex(u => u.Username).IsUnique();
        });

        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Property(a => a.Status).HasMaxLength(30).IsRequired();
            entity.Property(a => a.Type).HasMaxLength(50).IsRequired();
            entity.Property(a => a.Reason).HasMaxLength(500);
            entity.Property(a => a.Notes).HasMaxLength(2000);
            entity.Property(a => a.CancelledReason).HasMaxLength(500);

            entity.HasOne(a => a.Patient)
                .WithMany()
                .HasForeignKey(a => a.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(a => a.DoctorUser)
                .WithMany()
                .HasForeignKey(a => a.DoctorUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(a => a.PatientId);
            entity.HasIndex(a => a.DoctorUserId);
            entity.HasIndex(a => a.ScheduledAtUtc);
        });

        modelBuilder.Entity<ClinicalEncounter>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EncounterType).HasMaxLength(50).IsRequired();
            entity.Property(e => e.ChiefComplaint).HasMaxLength(500);
            entity.Property(e => e.HistoryOfPresentIllness).HasMaxLength(4000);
            entity.Property(e => e.Examination).HasMaxLength(4000);
            entity.Property(e => e.Assessment).HasMaxLength(4000);
            entity.Property(e => e.Plan).HasMaxLength(4000);
            entity.Property(e => e.FollowUpNotes).HasMaxLength(2000);

            entity.HasOne(e => e.Patient)
                .WithMany()
                .HasForeignKey(e => e.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Appointment)
                .WithMany()
                .HasForeignKey(e => e.AppointmentId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.AttendingDoctor)
                .WithMany()
                .HasForeignKey(e => e.AttendingDoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.PatientId);
            entity.HasIndex(e => e.AppointmentId);
        });

        modelBuilder.Entity<Diagnosis>(entity =>
        {
            entity.HasKey(d => d.Id);
            entity.Property(d => d.IcdCode).HasMaxLength(20).IsRequired();
            entity.Property(d => d.Description).HasMaxLength(500).IsRequired();
            entity.Property(d => d.DiagnosisType).HasMaxLength(20).IsRequired();

            entity.HasOne(d => d.Encounter)
                .WithMany(e => e.Diagnoses)
                .HasForeignKey(d => d.EncounterId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(d => d.EncounterId);
        });

        modelBuilder.Entity<VitalSigns>(entity =>
        {
            entity.HasKey(v => v.Id);
            entity.Property(v => v.HeightCm).HasPrecision(5, 1);
            entity.Property(v => v.WeightKg).HasPrecision(5, 1);
            entity.Property(v => v.TemperatureCelsius).HasPrecision(4, 1);
            entity.Property(v => v.OxygenSaturationPct).HasPrecision(5, 2);
            entity.Property(v => v.Notes).HasMaxLength(500);

            entity.HasOne(v => v.Encounter)
                .WithMany(e => e.VitalSigns)
                .HasForeignKey(v => v.EncounterId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(v => v.RecordedByUser)
                .WithMany()
                .HasForeignKey(v => v.RecordedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(v => v.EncounterId);
        });
    }
}
