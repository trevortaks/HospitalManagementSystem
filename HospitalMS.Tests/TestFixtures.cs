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
using LabOrderPriorityC = HospitalMS.Data.Persistence.Entities.LabOrderPriority;
using LabOrderStatusC = HospitalMS.Data.Persistence.Entities.LabOrderStatus;
using LabResultFlagC = HospitalMS.Data.Persistence.Entities.LabResultFlag;
using ImagingModalityC = HospitalMS.Data.Persistence.Entities.ImagingModality;
using ImagingRequestStatusC = HospitalMS.Data.Persistence.Entities.ImagingRequestStatus;

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

    public static LabOrderPanel CreateLabOrderPanel(
        Guid? id = null,
        string code = "CBC",
        string name = "Complete Blood Count",
        string? category = "Haematology",
        bool isActive = true)
    {
        return new LabOrderPanel
        {
            Id = id ?? Guid.NewGuid(),
            Code = code,
            Name = name,
            Category = category,
            IsActive = isActive,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public static LabOrder CreateLabOrder(
        Guid? id = null,
        Guid? encounterId = null,
        Guid? patientId = null,
        Guid? orderedByUserId = null,
        Guid? panelId = null,
        string priority = "Routine",
        string status = "Ordered")
    {
        return new LabOrder
        {
            Id = id ?? Guid.NewGuid(),
            EncounterId = encounterId ?? Guid.NewGuid(),
            PatientId = patientId ?? Guid.NewGuid(),
            OrderedByUserId = orderedByUserId ?? Guid.NewGuid(),
            PanelId = panelId ?? Guid.NewGuid(),
            Priority = priority,
            Status = status,
            OrderedAtUtc = DateTime.UtcNow
        };
    }

    public static LabResult CreateLabResult(
        Guid? id = null,
        Guid? orderId = null,
        Guid? recordedByUserId = null,
        string analyteName = "Haemoglobin",
        string value = "13.5",
        string? unit = "g/dL",
        string flag = "Normal")
    {
        return new LabResult
        {
            Id = id ?? Guid.NewGuid(),
            OrderId = orderId ?? Guid.NewGuid(),
            RecordedByUserId = recordedByUserId ?? Guid.NewGuid(),
            AnalyteName = analyteName,
            Value = value,
            Unit = unit,
            Flag = flag,
            RecordedAtUtc = DateTime.UtcNow
        };
    }

    public static ImagingRequest CreateImagingRequest(
        Guid? id = null,
        Guid? encounterId = null,
        Guid? patientId = null,
        Guid? requestedByUserId = null,
        string modality = "XRay",
        string? bodyPart = "Chest",
        string priority = "Routine",
        string status = "Requested")
    {
        return new ImagingRequest
        {
            Id = id ?? Guid.NewGuid(),
            EncounterId = encounterId ?? Guid.NewGuid(),
            PatientId = patientId ?? Guid.NewGuid(),
            RequestedByUserId = requestedByUserId ?? Guid.NewGuid(),
            Modality = modality,
            BodyPart = bodyPart,
            Priority = priority,
            Status = status,
            RequestedAtUtc = DateTime.UtcNow
        };
    }

    public static ImagingReport CreateImagingReport(
        Guid? id = null,
        Guid? requestId = null,
        Guid? radiologyUserId = null,
        string reportText = "No abnormality detected.",
        string? impression = "Normal")
    {
        return new ImagingReport
        {
            Id = id ?? Guid.NewGuid(),
            RequestId = requestId ?? Guid.NewGuid(),
            RadiologyUserId = radiologyUserId ?? Guid.NewGuid(),
            ReportText = reportText,
            Impression = impression,
            ReportedAtUtc = DateTime.UtcNow
        };
    }

    public static ChargeItem CreateChargeItem(
        Guid? id = null,
        string code = "CONSULT-GP",
        string description = "General Practitioner Consultation",
        string? category = "Consultation",
        decimal unitPrice = 50.00m,
        bool isActive = true)
    {
        return new ChargeItem
        {
            Id = id ?? Guid.NewGuid(),
            Code = code,
            Description = description,
            Category = category,
            UnitPrice = unitPrice,
            IsActive = isActive,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public static InsuranceProvider CreateInsuranceProvider(
        Guid? id = null,
        string name = "BlueCross Medical",
        string? contactPhone = "+1 800 000 0001",
        bool isActive = true)
    {
        return new InsuranceProvider
        {
            Id = id ?? Guid.NewGuid(),
            Name = name,
            ContactPhone = contactPhone,
            IsActive = isActive,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public static Invoice CreateInvoice(
        Guid? id = null,
        Guid? patientId = null,
        Guid? createdByUserId = null,
        string status = "Draft",
        decimal totalAmount = 0m)
    {
        return new Invoice
        {
            Id = id ?? Guid.NewGuid(),
            InvoiceNumber = $"INV-TEST-{Guid.NewGuid():N}"[..18].ToUpperInvariant(),
            PatientId = patientId ?? Guid.NewGuid(),
            CreatedByUserId = createdByUserId ?? Guid.NewGuid(),
            Status = status,
            TotalAmount = totalAmount,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };
    }

    public static InventoryCategory CreateInventoryCategory(
        Guid? id = null,
        string name = "Pharmaceuticals",
        string? description = "Medicines and drugs",
        bool isActive = true)
    {
        return new InventoryCategory
        {
            Id = id ?? Guid.NewGuid(),
            Name = name,
            Description = description,
            IsActive = isActive,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public static InventoryItem CreateInventoryItem(
        Guid? id = null,
        Guid? categoryId = null,
        string code = "MED-AMOX-500",
        string name = "Amoxicillin 500mg",
        string unit = "Tablet",
        int reorderLevel = 100,
        int currentStock = 500,
        decimal unitCost = 0.25m,
        bool isActive = true)
    {
        return new InventoryItem
        {
            Id = id ?? Guid.NewGuid(),
            CategoryId = categoryId ?? Guid.NewGuid(),
            Code = code,
            Name = name,
            Unit = unit,
            ReorderLevel = reorderLevel,
            CurrentStock = currentStock,
            UnitCost = unitCost,
            IsActive = isActive,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public static Ward CreateWard(
        Guid? id = null,
        string name = "General Ward A",
        string wardType = "General",
        int totalBeds = 10,
        int floorNumber = 1,
        bool isActive = true)
    {
        return new Ward
        {
            Id = id ?? Guid.NewGuid(),
            Name = name,
            WardType = wardType,
            TotalBeds = totalBeds,
            FloorNumber = floorNumber,
            IsActive = isActive,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public static Bed CreateBed(
        Guid? id = null,
        Guid? wardId = null,
        string bedNumber = "A1",
        string bedType = "Standard",
        string status = "Available",
        bool isActive = true)
    {
        return new Bed
        {
            Id = id ?? Guid.NewGuid(),
            WardId = wardId ?? Guid.NewGuid(),
            BedNumber = bedNumber,
            BedType = bedType,
            Status = status,
            IsActive = isActive,
            CreatedAtUtc = DateTime.UtcNow
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
