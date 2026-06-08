using HospitalMS.Data.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace HospitalMS.Data.Persistence;

public class HospitalDbContext(DbContextOptions<HospitalDbContext> options) : DbContext(options)
{
    // Npgsql requires Kind=Utc for timestamp with time zone. These converters normalize any
    // DateTime (including Kind=Unspecified from JSON deserialization) to UTC before writing.
    private sealed class UtcDateTimeConverter() : ValueConverter<DateTime, DateTime>(
        v => v.Kind == DateTimeKind.Utc ? v : DateTime.SpecifyKind(v, DateTimeKind.Utc),
        v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

    private sealed class NullableUtcDateTimeConverter() : ValueConverter<DateTime?, DateTime?>(
        v => v == null ? null : v.Value.Kind == DateTimeKind.Utc ? v : (DateTime?)DateTime.SpecifyKind(v.Value, DateTimeKind.Utc),
        v => v == null ? null : (DateTime?)DateTime.SpecifyKind(v.Value, DateTimeKind.Utc));

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<DateTime>().HaveConversion<UtcDateTimeConverter>();
        configurationBuilder.Properties<DateTime?>().HaveConversion<NullableUtcDateTimeConverter>();
    }

    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<ClinicalEncounter> ClinicalEncounters => Set<ClinicalEncounter>();
    public DbSet<Diagnosis> Diagnoses => Set<Diagnosis>();
    public DbSet<VitalSigns> VitalSigns => Set<VitalSigns>();
    public DbSet<Medication> Medications => Set<Medication>();
    public DbSet<Prescription> Prescriptions => Set<Prescription>();
    public DbSet<PatientPortalSession> PatientPortalSessions => Set<PatientPortalSession>();
    public DbSet<LabOrderPanel> LabOrderPanels => Set<LabOrderPanel>();
    public DbSet<LabOrder> LabOrders => Set<LabOrder>();
    public DbSet<LabResult> LabResults => Set<LabResult>();
    public DbSet<ImagingRequest> ImagingRequests => Set<ImagingRequest>();
    public DbSet<ImagingReport> ImagingReports => Set<ImagingReport>();
    public DbSet<ChargeItem> ChargeItems => Set<ChargeItem>();
    public DbSet<InsuranceProvider> InsuranceProviders => Set<InsuranceProvider>();
    public DbSet<PatientInsurance> PatientInsurances => Set<PatientInsurance>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceLineItem> InvoiceLineItems => Set<InvoiceLineItem>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<InsuranceClaim> InsuranceClaims => Set<InsuranceClaim>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
    public DbSet<PurchaseOrderLine> PurchaseOrderLines => Set<PurchaseOrderLine>();
    public DbSet<GoodsReceipt> GoodsReceipts => Set<GoodsReceipt>();
    public DbSet<GoodsReceiptLine> GoodsReceiptLines => Set<GoodsReceiptLine>();
    public DbSet<InventoryCategory> InventoryCategories => Set<InventoryCategory>();
    public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();
    public DbSet<StockTransaction> StockTransactions => Set<StockTransaction>();
    public DbSet<Ward> Wards => Set<Ward>();
    public DbSet<Bed> Beds => Set<Bed>();
    public DbSet<BedAllocation> BedAllocations => Set<BedAllocation>();
    public DbSet<AuditEntry> AuditEntries => Set<AuditEntry>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<Equipment> Equipment => Set<Equipment>();
    public DbSet<MaintenanceRequest> MaintenanceRequests => Set<MaintenanceRequest>();
    public DbSet<QualityIncident> QualityIncidents => Set<QualityIncident>();
    public DbSet<PatientFeedback> PatientFeedback => Set<PatientFeedback>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<EmployeeRecord> EmployeeRecords => Set<EmployeeRecord>();
    public DbSet<LeaveRequest> LeaveRequests => Set<LeaveRequest>();

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

            entity.HasOne(u => u.LinkedPatient)
                .WithMany()
                .HasForeignKey(u => u.LinkedPatientId)
                .OnDelete(DeleteBehavior.Restrict);
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

        modelBuilder.Entity<Medication>(entity =>
        {
            entity.HasKey(m => m.Id);
            entity.Property(m => m.GenericName).HasMaxLength(200).IsRequired();
            entity.Property(m => m.BrandName).HasMaxLength(200);
            entity.Property(m => m.Form).HasMaxLength(50).IsRequired();
            entity.Property(m => m.Strength).HasMaxLength(50);
            entity.Property(m => m.RouteOfAdministration).HasMaxLength(50);
            entity.HasIndex(m => m.GenericName);
        });

        modelBuilder.Entity<Prescription>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Dose).HasMaxLength(100).IsRequired();
            entity.Property(p => p.Frequency).HasMaxLength(100).IsRequired();
            entity.Property(p => p.Instructions).HasMaxLength(1000);
            entity.Property(p => p.Status).HasMaxLength(30).IsRequired();

            entity.HasOne(p => p.Encounter)
                .WithMany()
                .HasForeignKey(p => p.EncounterId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(p => p.Patient)
                .WithMany()
                .HasForeignKey(p => p.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(p => p.Medication)
                .WithMany()
                .HasForeignKey(p => p.MedicationId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(p => p.PrescribedByUser)
                .WithMany()
                .HasForeignKey(p => p.PrescribedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(p => p.DispensedByUser)
                .WithMany()
                .HasForeignKey(p => p.DispensedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(p => p.PatientId);
            entity.HasIndex(p => p.EncounterId);
            entity.HasIndex(p => p.Status);
        });

        modelBuilder.Entity<PatientPortalSession>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.Property(s => s.IpAddress).HasMaxLength(50);
            entity.Property(s => s.UserAgent).HasMaxLength(500);

            entity.HasOne(s => s.User)
                .WithMany()
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(s => s.Patient)
                .WithMany()
                .HasForeignKey(s => s.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(s => s.UserId);
            entity.HasIndex(s => s.PatientId);
        });

        modelBuilder.Entity<LabOrderPanel>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Code).HasMaxLength(50).IsRequired();
            entity.Property(p => p.Name).HasMaxLength(200).IsRequired();
            entity.Property(p => p.Category).HasMaxLength(100);
            entity.HasIndex(p => p.Code).IsUnique();
        });

        modelBuilder.Entity<LabOrder>(entity =>
        {
            entity.HasKey(o => o.Id);
            entity.Property(o => o.Priority).HasMaxLength(20).IsRequired();
            entity.Property(o => o.Status).HasMaxLength(20).IsRequired();
            entity.Property(o => o.Notes).HasMaxLength(1000);

            entity.HasOne(o => o.Encounter)
                .WithMany()
                .HasForeignKey(o => o.EncounterId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(o => o.Patient)
                .WithMany()
                .HasForeignKey(o => o.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(o => o.OrderedByUser)
                .WithMany()
                .HasForeignKey(o => o.OrderedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(o => o.Panel)
                .WithMany()
                .HasForeignKey(o => o.PanelId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(o => o.PatientId);
            entity.HasIndex(o => o.EncounterId);
            entity.HasIndex(o => o.Status);
        });

        modelBuilder.Entity<LabResult>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.AnalyteName).HasMaxLength(200).IsRequired();
            entity.Property(r => r.Value).HasMaxLength(200).IsRequired();
            entity.Property(r => r.Unit).HasMaxLength(50);
            entity.Property(r => r.ReferenceRange).HasMaxLength(100);
            entity.Property(r => r.Flag).HasMaxLength(10).IsRequired();

            entity.HasOne(r => r.Order)
                .WithMany(o => o.Results)
                .HasForeignKey(r => r.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(r => r.RecordedByUser)
                .WithMany()
                .HasForeignKey(r => r.RecordedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(r => r.OrderId);
        });

        modelBuilder.Entity<ImagingRequest>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.Modality).HasMaxLength(50).IsRequired();
            entity.Property(r => r.BodyPart).HasMaxLength(100);
            entity.Property(r => r.ClinicalIndication).HasMaxLength(1000);
            entity.Property(r => r.Priority).HasMaxLength(20).IsRequired();
            entity.Property(r => r.Status).HasMaxLength(30).IsRequired();

            entity.HasOne(r => r.Encounter)
                .WithMany()
                .HasForeignKey(r => r.EncounterId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(r => r.Patient)
                .WithMany()
                .HasForeignKey(r => r.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(r => r.RequestedByUser)
                .WithMany()
                .HasForeignKey(r => r.RequestedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(r => r.PatientId);
            entity.HasIndex(r => r.EncounterId);
            entity.HasIndex(r => r.Status);
        });

        modelBuilder.Entity<ImagingReport>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.ReportText).HasMaxLength(8000).IsRequired();
            entity.Property(r => r.Impression).HasMaxLength(2000);
            entity.Property(r => r.AttachmentPath).HasMaxLength(500);

            entity.HasOne(r => r.Request)
                .WithOne(req => req.Report)
                .HasForeignKey<ImagingReport>(r => r.RequestId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(r => r.RadiologyUser)
                .WithMany()
                .HasForeignKey(r => r.RadiologyUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ChargeItem>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Code).HasMaxLength(50).IsRequired();
            entity.Property(c => c.Description).HasMaxLength(500).IsRequired();
            entity.Property(c => c.Category).HasMaxLength(100);
            entity.Property(c => c.UnitPrice).HasPrecision(18, 2);
            entity.HasIndex(c => c.Code).IsUnique();
        });

        modelBuilder.Entity<InsuranceProvider>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Name).HasMaxLength(200).IsRequired();
            entity.Property(p => p.ContactPhone).HasMaxLength(30);
            entity.Property(p => p.ContactEmail).HasMaxLength(256);
            entity.Property(p => p.Address).HasMaxLength(500);
        });

        modelBuilder.Entity<PatientInsurance>(entity =>
        {
            entity.HasKey(pi => pi.Id);
            entity.Property(pi => pi.PolicyNumber).HasMaxLength(100).IsRequired();
            entity.Property(pi => pi.GroupNumber).HasMaxLength(100);

            entity.HasOne(pi => pi.Patient)
                .WithMany()
                .HasForeignKey(pi => pi.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(pi => pi.Provider)
                .WithMany()
                .HasForeignKey(pi => pi.ProviderId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(pi => pi.PatientId);
        });

        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasKey(i => i.Id);
            entity.Property(i => i.InvoiceNumber).HasMaxLength(50).IsRequired();
            entity.Property(i => i.Status).HasMaxLength(30).IsRequired();
            entity.Property(i => i.SubtotalAmount).HasPrecision(18, 2);
            entity.Property(i => i.DiscountAmount).HasPrecision(18, 2);
            entity.Property(i => i.TaxAmount).HasPrecision(18, 2);
            entity.Property(i => i.TotalAmount).HasPrecision(18, 2);
            entity.Property(i => i.PaidAmount).HasPrecision(18, 2);
            entity.Property(i => i.Notes).HasMaxLength(1000);
            entity.HasIndex(i => i.InvoiceNumber).IsUnique();

            entity.HasOne(i => i.Patient)
                .WithMany()
                .HasForeignKey(i => i.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(i => i.Encounter)
                .WithMany()
                .HasForeignKey(i => i.EncounterId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(i => i.CreatedByUser)
                .WithMany()
                .HasForeignKey(i => i.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(i => i.PatientId);
            entity.HasIndex(i => i.Status);
        });

        modelBuilder.Entity<InvoiceLineItem>(entity =>
        {
            entity.HasKey(li => li.Id);
            entity.Property(li => li.Description).HasMaxLength(500).IsRequired();
            entity.Property(li => li.UnitPrice).HasPrecision(18, 2);
            entity.Property(li => li.TotalPrice).HasPrecision(18, 2);

            entity.HasOne(li => li.Invoice)
                .WithMany(i => i.LineItems)
                .HasForeignKey(li => li.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(li => li.ChargeItem)
                .WithMany()
                .HasForeignKey(li => li.ChargeItemId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(li => li.InvoiceId);
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Amount).HasPrecision(18, 2);
            entity.Property(p => p.Method).HasMaxLength(30).IsRequired();
            entity.Property(p => p.ReferenceNumber).HasMaxLength(100);
            entity.Property(p => p.Notes).HasMaxLength(500);

            entity.HasOne(p => p.Invoice)
                .WithMany(i => i.Payments)
                .HasForeignKey(p => p.InvoiceId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(p => p.RecordedByUser)
                .WithMany()
                .HasForeignKey(p => p.RecordedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(p => p.InvoiceId);
        });

        modelBuilder.Entity<InsuranceClaim>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.ClaimNumber).HasMaxLength(100).IsRequired();
            entity.Property(c => c.Status).HasMaxLength(30).IsRequired();
            entity.Property(c => c.ApprovedAmount).HasPrecision(18, 2);
            entity.Property(c => c.RejectionReason).HasMaxLength(500);

            entity.HasOne(c => c.Invoice)
                .WithMany(i => i.Claims)
                .HasForeignKey(c => c.InvoiceId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(c => c.PatientInsurance)
                .WithMany()
                .HasForeignKey(c => c.PatientInsuranceId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(c => c.InvoiceId);
            entity.HasIndex(c => c.ClaimNumber).IsUnique();
        });

        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.Property(s => s.Name).HasMaxLength(200).IsRequired();
            entity.Property(s => s.ContactName).HasMaxLength(100);
            entity.Property(s => s.ContactPhone).HasMaxLength(30);
            entity.Property(s => s.ContactEmail).HasMaxLength(256);
            entity.Property(s => s.Address).HasMaxLength(500);
        });

        modelBuilder.Entity<PurchaseOrder>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.OrderNumber).HasMaxLength(30).IsRequired();
            entity.Property(p => p.Status).HasMaxLength(30).IsRequired();
            entity.Property(p => p.Notes).HasMaxLength(1000);

            entity.HasOne(p => p.Supplier)
                .WithMany(s => s.PurchaseOrders)
                .HasForeignKey(p => p.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(p => p.OrderedByUser)
                .WithMany()
                .HasForeignKey(p => p.OrderedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(p => p.OrderNumber).IsUnique();
            entity.HasIndex(p => p.SupplierId);
            entity.HasIndex(p => p.Status);
        });

        modelBuilder.Entity<PurchaseOrderLine>(entity =>
        {
            entity.HasKey(l => l.Id);
            entity.Property(l => l.Description).HasMaxLength(500).IsRequired();
            entity.Property(l => l.UnitCost).HasPrecision(18, 2);
            entity.Property(l => l.TotalCost).HasPrecision(18, 2);

            entity.HasOne(l => l.PurchaseOrder)
                .WithMany(p => p.Lines)
                .HasForeignKey(l => l.PurchaseOrderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(l => l.InventoryItem)
                .WithMany()
                .HasForeignKey(l => l.InventoryItemId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(l => l.PurchaseOrderId);
        });

        modelBuilder.Entity<GoodsReceipt>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.Notes).HasMaxLength(1000);

            entity.HasOne(r => r.PurchaseOrder)
                .WithMany(p => p.Receipts)
                .HasForeignKey(r => r.PurchaseOrderId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(r => r.ReceivedByUser)
                .WithMany()
                .HasForeignKey(r => r.ReceivedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(r => r.PurchaseOrderId);
        });

        modelBuilder.Entity<GoodsReceiptLine>(entity =>
        {
            entity.HasKey(l => l.Id);

            entity.HasOne(l => l.Receipt)
                .WithMany(r => r.Lines)
                .HasForeignKey(l => l.GoodsReceiptId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(l => l.PurchaseOrderLine)
                .WithMany()
                .HasForeignKey(l => l.PurchaseOrderLineId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<InventoryCategory>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Name).HasMaxLength(100).IsRequired();
            entity.Property(c => c.Description).HasMaxLength(500);
            entity.HasIndex(c => c.Name).IsUnique();
        });

        modelBuilder.Entity<InventoryItem>(entity =>
        {
            entity.HasKey(i => i.Id);
            entity.Property(i => i.Code).HasMaxLength(50).IsRequired();
            entity.Property(i => i.Name).HasMaxLength(200).IsRequired();
            entity.Property(i => i.Description).HasMaxLength(500);
            entity.Property(i => i.Unit).HasMaxLength(50).IsRequired();
            entity.Property(i => i.UnitCost).HasPrecision(18, 2);

            entity.HasOne(i => i.Category)
                .WithMany(c => c.Items)
                .HasForeignKey(i => i.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(i => i.Code).IsUnique();
            entity.HasIndex(i => i.CategoryId);
        });

        modelBuilder.Entity<StockTransaction>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Type).HasMaxLength(20).IsRequired();
            entity.Property(t => t.ReferenceType).HasMaxLength(50);
            entity.Property(t => t.Notes).HasMaxLength(500);

            entity.HasOne(t => t.Item)
                .WithMany(i => i.Transactions)
                .HasForeignKey(t => t.ItemId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(t => t.PerformedByUser)
                .WithMany()
                .HasForeignKey(t => t.PerformedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(t => t.ItemId);
            entity.HasIndex(t => t.TransactedAtUtc);
        });

        modelBuilder.Entity<Ward>(entity =>
        {
            entity.HasKey(w => w.Id);
            entity.Property(w => w.Name).HasMaxLength(100).IsRequired();
            entity.Property(w => w.WardType).HasMaxLength(30).IsRequired();
        });

        modelBuilder.Entity<Bed>(entity =>
        {
            entity.HasKey(b => b.Id);
            entity.Property(b => b.BedNumber).HasMaxLength(20).IsRequired();
            entity.Property(b => b.BedType).HasMaxLength(30).IsRequired();
            entity.Property(b => b.Status).HasMaxLength(30).IsRequired();

            entity.HasOne(b => b.Ward)
                .WithMany(w => w.Beds)
                .HasForeignKey(b => b.WardId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(b => new { b.WardId, b.BedNumber }).IsUnique();
        });

        modelBuilder.Entity<BedAllocation>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Property(a => a.DischargeReason).HasMaxLength(500);

            entity.HasOne(a => a.Bed)
                .WithMany(b => b.Allocations)
                .HasForeignKey(a => a.BedId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(a => a.Patient)
                .WithMany()
                .HasForeignKey(a => a.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(a => a.Encounter)
                .WithMany()
                .HasForeignKey(a => a.EncounterId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(a => a.AdmittedByUser)
                .WithMany()
                .HasForeignKey(a => a.AdmittedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(a => a.DischargedByUser)
                .WithMany()
                .HasForeignKey(a => a.DischargedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(a => a.TransferredToBed)
                .WithMany()
                .HasForeignKey(a => a.TransferredToBedId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(a => a.PatientId);
            entity.HasIndex(a => a.BedId);
        });

        modelBuilder.Entity<AuditEntry>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Property(a => a.EntityType).HasMaxLength(100).IsRequired();
            entity.Property(a => a.Action).HasMaxLength(50).IsRequired();
            entity.Property(a => a.Details).HasMaxLength(2000);
            entity.Property(a => a.PerformedByUsername).HasMaxLength(100);
            entity.Property(a => a.IpAddress).HasMaxLength(45);

            entity.HasIndex(a => a.EntityType);
            entity.HasIndex(a => a.PerformedByUserId);
            entity.HasIndex(a => a.PerformedAtUtc);
        });

        modelBuilder.Entity<Room>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.Name).HasMaxLength(200).IsRequired();
            entity.Property(r => r.RoomNumber).HasMaxLength(20).IsRequired();
            entity.Property(r => r.RoomType).HasMaxLength(50).IsRequired();
            entity.Property(r => r.Building).HasMaxLength(100);
            entity.Property(r => r.Notes).HasMaxLength(500);
            entity.HasIndex(r => r.RoomNumber).IsUnique();
        });

        modelBuilder.Entity<Equipment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Code).HasMaxLength(50).IsRequired();
            entity.Property(e => e.EquipmentType).HasMaxLength(100).IsRequired();
            entity.Property(e => e.SerialNumber).HasMaxLength(100);
            entity.Property(e => e.Manufacturer).HasMaxLength(200);
            entity.Property(e => e.Model).HasMaxLength(200);
            entity.Property(e => e.Status).HasMaxLength(30).IsRequired();
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.HasIndex(e => e.Code).IsUnique();

            entity.HasOne(e => e.LocationRoom)
                .WithMany()
                .HasForeignKey(e => e.LocationRoomId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<MaintenanceRequest>(entity =>
        {
            entity.HasKey(m => m.Id);
            entity.Property(m => m.Title).HasMaxLength(200).IsRequired();
            entity.Property(m => m.Description).HasMaxLength(2000).IsRequired();
            entity.Property(m => m.RequestType).HasMaxLength(30).IsRequired();
            entity.Property(m => m.Priority).HasMaxLength(20).IsRequired();
            entity.Property(m => m.Status).HasMaxLength(20).IsRequired();
            entity.Property(m => m.ResolutionNotes).HasMaxLength(2000);

            entity.HasOne(m => m.Room)
                .WithMany(r => r.MaintenanceRequests)
                .HasForeignKey(m => m.RoomId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(m => m.Equipment)
                .WithMany(e => e.MaintenanceRequests)
                .HasForeignKey(m => m.EquipmentId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(m => m.RequestedByUser)
                .WithMany()
                .HasForeignKey(m => m.RequestedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(m => m.AssignedToUser)
                .WithMany()
                .HasForeignKey(m => m.AssignedToUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(m => m.Status);
            entity.HasIndex(m => m.RequestedAtUtc);
        });

        modelBuilder.Entity<QualityIncident>(entity =>
        {
            entity.HasKey(q => q.Id);
            entity.Property(q => q.Title).HasMaxLength(200).IsRequired();
            entity.Property(q => q.Description).HasMaxLength(2000).IsRequired();
            entity.Property(q => q.IncidentType).HasMaxLength(50).IsRequired();
            entity.Property(q => q.Severity).HasMaxLength(20).IsRequired();
            entity.Property(q => q.Status).HasMaxLength(30).IsRequired();
            entity.Property(q => q.Location).HasMaxLength(200);
            entity.Property(q => q.RootCause).HasMaxLength(2000);
            entity.Property(q => q.CorrectiveAction).HasMaxLength(2000);

            entity.HasOne(q => q.ReportedByUser)
                .WithMany()
                .HasForeignKey(q => q.ReportedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(q => q.AssignedToUser)
                .WithMany()
                .HasForeignKey(q => q.AssignedToUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(q => q.Patient)
                .WithMany()
                .HasForeignKey(q => q.PatientId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(q => q.Status);
            entity.HasIndex(q => q.OccurredAtUtc);
        });

        modelBuilder.Entity<PatientFeedback>(entity =>
        {
            entity.HasKey(f => f.Id);
            entity.Property(f => f.Comments).HasMaxLength(2000);

            entity.HasOne(f => f.Patient)
                .WithMany()
                .HasForeignKey(f => f.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(f => f.Appointment)
                .WithMany()
                .HasForeignKey(f => f.AppointmentId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(f => f.PatientId);
            entity.HasIndex(f => f.SubmittedAtUtc);
        });

        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasKey(d => d.Id);
            entity.Property(d => d.Name).HasMaxLength(200).IsRequired();
            entity.Property(d => d.Description).HasMaxLength(1000);
            entity.HasIndex(d => d.Name).IsUnique();

            entity.HasOne(d => d.HeadUser)
                .WithMany()
                .HasForeignKey(d => d.HeadUserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<EmployeeRecord>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EmployeeNumber).HasMaxLength(50).IsRequired();
            entity.Property(e => e.JobTitle).HasMaxLength(200).IsRequired();
            entity.Property(e => e.EmploymentType).HasMaxLength(30).IsRequired();
            entity.Property(e => e.Status).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Notes).HasMaxLength(2000);
            entity.Property(e => e.Salary).HasPrecision(18, 2);
            entity.HasIndex(e => e.EmployeeNumber).IsUnique();
            entity.HasIndex(e => e.UserId).IsUnique();

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Department)
                .WithMany(d => d.Employees)
                .HasForeignKey(e => e.DepartmentId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.DepartmentId);
        });

        modelBuilder.Entity<LeaveRequest>(entity =>
        {
            entity.HasKey(l => l.Id);
            entity.Property(l => l.LeaveType).HasMaxLength(30).IsRequired();
            entity.Property(l => l.Status).HasMaxLength(20).IsRequired();
            entity.Property(l => l.Reason).HasMaxLength(1000);
            entity.Property(l => l.ReviewNotes).HasMaxLength(1000);

            entity.HasOne(l => l.EmployeeRecord)
                .WithMany(e => e.LeaveRequests)
                .HasForeignKey(l => l.EmployeeRecordId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(l => l.ReviewedByUser)
                .WithMany()
                .HasForeignKey(l => l.ReviewedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(l => l.Status);
            entity.HasIndex(l => l.EmployeeRecordId);
        });
    }
}
