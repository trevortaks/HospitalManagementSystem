using HospitalMS.Common.Constants;
using HospitalMS.Data.Persistence;
using HospitalMS.Data.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using AppointmentStatus = HospitalMS.Data.Persistence.Entities.AppointmentStatus;
using AppointmentType = HospitalMS.Data.Persistence.Entities.AppointmentType;
using EncounterType = HospitalMS.Data.Persistence.Entities.EncounterType;
using MedicationForm = HospitalMS.Data.Persistence.Entities.MedicationForm;
using PrescriptionStatus = HospitalMS.Data.Persistence.Entities.PrescriptionStatus;
using PatientPortalSession = HospitalMS.Data.Persistence.Entities.PatientPortalSession;

namespace HospitalMS.Tests;

public static class TestFixtures
{
    public static string CreateDatabaseName() => $"{ApplicationInfo.ApplicationName}-tests-{Guid.NewGuid():N}";

    public static DbContextOptions<HospitalDbContext> CreateInMemoryOptions(string? databaseName = null)
        => new DbContextOptionsBuilder<HospitalDbContext>()
            .UseInMemoryDatabase(databaseName ?? CreateDatabaseName())
            .EnableSensitiveDataLogging()
            .Options;

    public static Patient CreatePatient(
        Guid? id = null,
        string? medicalRecordNumber = null,
        string firstName = "Jane",
        string lastName = "Doe",
        string? email = null)
    {
        var patientId = id ?? Guid.NewGuid();

        return new Patient
        {
            Id = patientId,
            MedicalRecordNumber = medicalRecordNumber ?? $"MRN-{patientId:N}"[..16],
            FirstName = firstName,
            LastName = lastName,
            DateOfBirth = new DateTime(1990, 1, 1),
            Email = email ?? $"{firstName.ToLowerInvariant()}.{lastName.ToLowerInvariant()}@example.test",
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public static IReadOnlyList<Patient> CreatePatients(int count)
        => Enumerable.Range(1, count)
            .Select(index => CreatePatient(firstName: $"Patient{index}", lastName: "Test"))
            .ToArray();

    public static Appointment CreateAppointment(
        Guid? id = null,
        Guid? patientId = null,
        Guid? doctorUserId = null,
        string status = AppointmentStatus.Scheduled,
        string type = AppointmentType.General)
    {
        return new Appointment
        {
            Id = id ?? Guid.NewGuid(),
            PatientId = patientId ?? Guid.NewGuid(),
            DoctorUserId = doctorUserId ?? Guid.NewGuid(),
            ScheduledAtUtc = DateTime.UtcNow.AddDays(1),
            DurationMinutes = 30,
            Status = status,
            Type = type,
            Reason = "Routine checkup",
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public static ClinicalEncounter CreateEncounter(
        Guid? id = null,
        Guid? patientId = null,
        Guid? attendingDoctorId = null,
        Guid? appointmentId = null,
        bool isClosed = false)
    {
        return new ClinicalEncounter
        {
            Id = id ?? Guid.NewGuid(),
            PatientId = patientId ?? Guid.NewGuid(),
            AttendingDoctorId = attendingDoctorId ?? Guid.NewGuid(),
            AppointmentId = appointmentId,
            EncounterType = EncounterType.Outpatient,
            ChiefComplaint = "Headache",
            IsClosed = isClosed,
            StartedAtUtc = DateTime.UtcNow,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public static Medication CreateMedication(
        Guid? id = null,
        string genericName = "Amoxicillin",
        string form = "Tablet",
        string? brandName = "Amoxil",
        string? strength = "500 mg",
        bool isControlled = false,
        bool isActive = true)
    {
        return new Medication
        {
            Id = id ?? Guid.NewGuid(),
            GenericName = genericName,
            BrandName = brandName,
            Form = form,
            Strength = strength,
            IsControlled = isControlled,
            IsActive = isActive,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public static Prescription CreatePrescription(
        Guid? id = null,
        Guid? encounterId = null,
        Guid? patientId = null,
        Guid? prescribedByUserId = null,
        Guid? medicationId = null,
        string status = "Active")
    {
        return new Prescription
        {
            Id = id ?? Guid.NewGuid(),
            EncounterId = encounterId ?? Guid.NewGuid(),
            PatientId = patientId ?? Guid.NewGuid(),
            PrescribedByUserId = prescribedByUserId ?? Guid.NewGuid(),
            MedicationId = medicationId ?? Guid.NewGuid(),
            Dose = "500 mg",
            Frequency = "TDS",
            DurationDays = 7,
            Status = status,
            PrescribedAtUtc = DateTime.UtcNow
        };
    }

    public static PatientPortalSession CreatePortalSession(
        Guid? id = null,
        Guid? userId = null,
        Guid? patientId = null,
        string? ipAddress = "127.0.0.1")
    {
        return new PatientPortalSession
        {
            Id = id ?? Guid.NewGuid(),
            UserId = userId ?? Guid.NewGuid(),
            PatientId = patientId ?? Guid.NewGuid(),
            IpAddress = ipAddress,
            LoginAtUtc = DateTime.UtcNow,
            LastActivityAtUtc = DateTime.UtcNow
        };
    }

    public static User CreateUser(
        Guid? id = null,
        string? username = null,
        string? email = null,
        string role = "Administrator")
    {
        var userId = id ?? Guid.NewGuid();

        return new User
        {
            Id = userId,
            Username = username ?? $"user_{userId:N}"[..12],
            Email = email ?? $"user_{userId:N}@example.test",
            Role = role,
            PasswordHash = string.Empty,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };
    }
}
