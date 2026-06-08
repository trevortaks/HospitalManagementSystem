using HospitalMS.Data.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HospitalMS.Data.Extensions;

public static class DataAccessExtensions
{
    public static async Task InitializeHospitalDatabaseAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(services);

        await using var scope = services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<HospitalDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger(nameof(DataAccessExtensions));

        logger.LogInformation("Initializing database schema...");

        foreach (var sql in SchemaDdl)
            await context.Database.ExecuteSqlRawAsync(sql, cancellationToken);

        logger.LogInformation("Database schema ready.");
    }

    private static readonly string[] SchemaDdl =
    [
        """
        CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
            "MigrationId" character varying(150) NOT NULL,
            "ProductVersion" character varying(32) NOT NULL,
            CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
        )
        """,
        """
        CREATE TABLE IF NOT EXISTS "Patients" (
            "Id" uuid NOT NULL,
            "MedicalRecordNumber" character varying(32) NOT NULL,
            "FirstName" character varying(100) NOT NULL,
            "LastName" character varying(100) NOT NULL,
            "DateOfBirth" timestamp with time zone NOT NULL,
            "Email" character varying(256) NOT NULL,
            "CreatedAtUtc" timestamp with time zone NOT NULL,
            CONSTRAINT "PK_Patients" PRIMARY KEY ("Id")
        )
        """,
        """
        CREATE TABLE IF NOT EXISTS "Users" (
            "Id" uuid NOT NULL,
            "Username" character varying(100) NOT NULL,
            "Email" character varying(256) NOT NULL,
            "Role" character varying(100) NOT NULL,
            "IsActive" boolean NOT NULL,
            "CreatedAtUtc" timestamp with time zone NOT NULL,
            "PasswordHash" character varying(100) NOT NULL DEFAULT '',
            CONSTRAINT "PK_Users" PRIMARY KEY ("Id")
        )
        """,
        """CREATE UNIQUE INDEX IF NOT EXISTS "IX_Patients_MedicalRecordNumber" ON "Patients" ("MedicalRecordNumber")""",
        """CREATE UNIQUE INDEX IF NOT EXISTS "IX_Users_Username" ON "Users" ("Username")""",
        """
        INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
        VALUES ('20260607000000_RemoveMultiTenancy', '10.0.8')
        ON CONFLICT ("MigrationId") DO NOTHING
        """,

        // Phase 1 — Patient demographics enrichment
        """ALTER TABLE "Patients" ADD COLUMN IF NOT EXISTS "PhoneNumber" character varying(20)""",
        """ALTER TABLE "Patients" ADD COLUMN IF NOT EXISTS "Gender" character varying(20)""",
        """ALTER TABLE "Patients" ADD COLUMN IF NOT EXISTS "BloodGroup" character varying(5)""",
        """ALTER TABLE "Patients" ADD COLUMN IF NOT EXISTS "AddressLine1" character varying(200)""",
        """ALTER TABLE "Patients" ADD COLUMN IF NOT EXISTS "City" character varying(100)""",
        """ALTER TABLE "Patients" ADD COLUMN IF NOT EXISTS "PostalCode" character varying(20)""",
        """ALTER TABLE "Patients" ADD COLUMN IF NOT EXISTS "Country" character varying(100)""",
        """ALTER TABLE "Patients" ADD COLUMN IF NOT EXISTS "EmergencyContactName" character varying(200)""",
        """ALTER TABLE "Patients" ADD COLUMN IF NOT EXISTS "EmergencyContactPhone" character varying(20)""",
        """ALTER TABLE "Patients" ADD COLUMN IF NOT EXISTS "UpdatedAtUtc" timestamp with time zone""",

        // Phase 1 — Appointments table
        """
        CREATE TABLE IF NOT EXISTS "Appointments" (
            "Id" uuid NOT NULL,
            "PatientId" uuid NOT NULL,
            "DoctorUserId" uuid NOT NULL,
            "ScheduledAtUtc" timestamp with time zone NOT NULL,
            "DurationMinutes" integer NOT NULL DEFAULT 30,
            "Status" character varying(30) NOT NULL DEFAULT 'Scheduled',
            "Type" character varying(50) NOT NULL DEFAULT 'General',
            "Reason" character varying(500),
            "Notes" character varying(2000),
            "CancelledReason" character varying(500),
            "CreatedAtUtc" timestamp with time zone NOT NULL,
            "UpdatedAtUtc" timestamp with time zone,
            CONSTRAINT "PK_Appointments" PRIMARY KEY ("Id"),
            CONSTRAINT "FK_Appointments_Patients" FOREIGN KEY ("PatientId") REFERENCES "Patients" ("Id") ON DELETE RESTRICT,
            CONSTRAINT "FK_Appointments_Users" FOREIGN KEY ("DoctorUserId") REFERENCES "Users" ("Id") ON DELETE RESTRICT
        )
        """,
        """CREATE INDEX IF NOT EXISTS "IX_Appointments_PatientId" ON "Appointments" ("PatientId")""",
        """CREATE INDEX IF NOT EXISTS "IX_Appointments_DoctorUserId" ON "Appointments" ("DoctorUserId")""",
        """CREATE INDEX IF NOT EXISTS "IX_Appointments_ScheduledAtUtc" ON "Appointments" ("ScheduledAtUtc")""",

        // Phase 1 — ClinicalEncounters table
        """
        CREATE TABLE IF NOT EXISTS "ClinicalEncounters" (
            "Id" uuid NOT NULL,
            "PatientId" uuid NOT NULL,
            "AppointmentId" uuid,
            "AttendingDoctorId" uuid NOT NULL,
            "EncounterType" character varying(50) NOT NULL,
            "StartedAtUtc" timestamp with time zone NOT NULL,
            "EndedAtUtc" timestamp with time zone,
            "ChiefComplaint" character varying(500),
            "HistoryOfPresentIllness" character varying(4000),
            "Examination" character varying(4000),
            "Assessment" character varying(4000),
            "Plan" character varying(4000),
            "FollowUpNotes" character varying(2000),
            "IsClosed" boolean NOT NULL DEFAULT false,
            "CreatedAtUtc" timestamp with time zone NOT NULL,
            "UpdatedAtUtc" timestamp with time zone,
            CONSTRAINT "PK_ClinicalEncounters" PRIMARY KEY ("Id"),
            CONSTRAINT "FK_ClinicalEncounters_Patients" FOREIGN KEY ("PatientId") REFERENCES "Patients" ("Id") ON DELETE RESTRICT,
            CONSTRAINT "FK_ClinicalEncounters_Appointments" FOREIGN KEY ("AppointmentId") REFERENCES "Appointments" ("Id") ON DELETE SET NULL,
            CONSTRAINT "FK_ClinicalEncounters_Users" FOREIGN KEY ("AttendingDoctorId") REFERENCES "Users" ("Id") ON DELETE RESTRICT
        )
        """,
        """CREATE INDEX IF NOT EXISTS "IX_ClinicalEncounters_PatientId" ON "ClinicalEncounters" ("PatientId")""",
        """CREATE INDEX IF NOT EXISTS "IX_ClinicalEncounters_AppointmentId" ON "ClinicalEncounters" ("AppointmentId")""",

        // Phase 1 — Diagnoses table
        """
        CREATE TABLE IF NOT EXISTS "Diagnoses" (
            "Id" uuid NOT NULL,
            "EncounterId" uuid NOT NULL,
            "IcdCode" character varying(20) NOT NULL,
            "Description" character varying(500) NOT NULL,
            "DiagnosisType" character varying(20) NOT NULL DEFAULT 'Primary',
            "CreatedAtUtc" timestamp with time zone NOT NULL,
            CONSTRAINT "PK_Diagnoses" PRIMARY KEY ("Id"),
            CONSTRAINT "FK_Diagnoses_ClinicalEncounters" FOREIGN KEY ("EncounterId") REFERENCES "ClinicalEncounters" ("Id") ON DELETE CASCADE
        )
        """,
        """CREATE INDEX IF NOT EXISTS "IX_Diagnoses_EncounterId" ON "Diagnoses" ("EncounterId")""",

        // Phase 1 — VitalSigns table
        """
        CREATE TABLE IF NOT EXISTS "VitalSigns" (
            "Id" uuid NOT NULL,
            "EncounterId" uuid NOT NULL,
            "RecordedByUserId" uuid NOT NULL,
            "RecordedAtUtc" timestamp with time zone NOT NULL,
            "HeightCm" numeric(5,1),
            "WeightKg" numeric(5,1),
            "TemperatureCelsius" numeric(4,1),
            "BloodPressureSystolic" integer,
            "BloodPressureDiastolic" integer,
            "HeartRateBpm" integer,
            "RespiratoryRate" integer,
            "OxygenSaturationPct" numeric(5,2),
            "Notes" character varying(500),
            CONSTRAINT "PK_VitalSigns" PRIMARY KEY ("Id"),
            CONSTRAINT "FK_VitalSigns_ClinicalEncounters" FOREIGN KEY ("EncounterId") REFERENCES "ClinicalEncounters" ("Id") ON DELETE CASCADE,
            CONSTRAINT "FK_VitalSigns_Users" FOREIGN KEY ("RecordedByUserId") REFERENCES "Users" ("Id") ON DELETE RESTRICT
        )
        """,
        """CREATE INDEX IF NOT EXISTS "IX_VitalSigns_EncounterId" ON "VitalSigns" ("EncounterId")""",

        """
        INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
        VALUES ('20260607000001_Phase1_AppointmentsAndEHR', '10.0.8')
        ON CONFLICT ("MigrationId") DO NOTHING
        """,

        // Phase 2 — Medications catalogue
        """
        CREATE TABLE IF NOT EXISTS "Medications" (
            "Id" uuid NOT NULL,
            "GenericName" character varying(200) NOT NULL,
            "BrandName" character varying(200),
            "Form" character varying(50) NOT NULL,
            "Strength" character varying(50),
            "RouteOfAdministration" character varying(50),
            "IsControlled" boolean NOT NULL DEFAULT false,
            "IsActive" boolean NOT NULL DEFAULT true,
            "CreatedAtUtc" timestamp with time zone NOT NULL,
            CONSTRAINT "PK_Medications" PRIMARY KEY ("Id")
        )
        """,
        """CREATE INDEX IF NOT EXISTS "IX_Medications_GenericName" ON "Medications" ("GenericName")""",

        // Phase 2 — Prescriptions
        """
        CREATE TABLE IF NOT EXISTS "Prescriptions" (
            "Id" uuid NOT NULL,
            "EncounterId" uuid NOT NULL,
            "PatientId" uuid NOT NULL,
            "PrescribedByUserId" uuid NOT NULL,
            "MedicationId" uuid NOT NULL,
            "Dose" character varying(100) NOT NULL,
            "Frequency" character varying(100) NOT NULL,
            "DurationDays" integer,
            "QuantityDispensed" integer,
            "Instructions" character varying(1000),
            "Status" character varying(30) NOT NULL DEFAULT 'Active',
            "PrescribedAtUtc" timestamp with time zone NOT NULL,
            "DispensedAtUtc" timestamp with time zone,
            "DispensedByUserId" uuid,
            CONSTRAINT "PK_Prescriptions" PRIMARY KEY ("Id"),
            CONSTRAINT "FK_Prescriptions_ClinicalEncounters" FOREIGN KEY ("EncounterId") REFERENCES "ClinicalEncounters" ("Id") ON DELETE RESTRICT,
            CONSTRAINT "FK_Prescriptions_Patients" FOREIGN KEY ("PatientId") REFERENCES "Patients" ("Id") ON DELETE RESTRICT,
            CONSTRAINT "FK_Prescriptions_Medications" FOREIGN KEY ("MedicationId") REFERENCES "Medications" ("Id") ON DELETE RESTRICT,
            CONSTRAINT "FK_Prescriptions_PrescribedBy" FOREIGN KEY ("PrescribedByUserId") REFERENCES "Users" ("Id") ON DELETE RESTRICT,
            CONSTRAINT "FK_Prescriptions_DispensedBy" FOREIGN KEY ("DispensedByUserId") REFERENCES "Users" ("Id") ON DELETE RESTRICT
        )
        """,
        """CREATE INDEX IF NOT EXISTS "IX_Prescriptions_PatientId" ON "Prescriptions" ("PatientId")""",
        """CREATE INDEX IF NOT EXISTS "IX_Prescriptions_EncounterId" ON "Prescriptions" ("EncounterId")""",
        """CREATE INDEX IF NOT EXISTS "IX_Prescriptions_Status" ON "Prescriptions" ("Status")""",

        """
        INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
        VALUES ('20260607000002_Phase2_Medications', '10.0.8')
        ON CONFLICT ("MigrationId") DO NOTHING
        """,

        // Phase 3 — Patient Self-Service Portal
        """ALTER TABLE "Users" ADD COLUMN IF NOT EXISTS "LinkedPatientId" uuid""",
        """
        DO $$
        BEGIN
            IF NOT EXISTS (
                SELECT 1 FROM pg_constraint
                WHERE conname = 'FK_Users_LinkedPatient'
            ) THEN
                ALTER TABLE "Users"
                ADD CONSTRAINT "FK_Users_LinkedPatient"
                FOREIGN KEY ("LinkedPatientId") REFERENCES "Patients" ("Id") ON DELETE RESTRICT;
            END IF;
        END $$
        """,
        """CREATE INDEX IF NOT EXISTS "IX_Users_LinkedPatientId" ON "Users" ("LinkedPatientId") WHERE "LinkedPatientId" IS NOT NULL""",

        """
        CREATE TABLE IF NOT EXISTS "PatientPortalSessions" (
            "Id" uuid NOT NULL,
            "UserId" uuid NOT NULL,
            "PatientId" uuid NOT NULL,
            "IpAddress" character varying(50),
            "UserAgent" character varying(500),
            "LoginAtUtc" timestamp with time zone NOT NULL,
            "LastActivityAtUtc" timestamp with time zone NOT NULL,
            CONSTRAINT "PK_PatientPortalSessions" PRIMARY KEY ("Id"),
            CONSTRAINT "FK_PatientPortalSessions_Users" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE RESTRICT,
            CONSTRAINT "FK_PatientPortalSessions_Patients" FOREIGN KEY ("PatientId") REFERENCES "Patients" ("Id") ON DELETE RESTRICT
        )
        """,
        """CREATE INDEX IF NOT EXISTS "IX_PatientPortalSessions_UserId" ON "PatientPortalSessions" ("UserId")""",
        """CREATE INDEX IF NOT EXISTS "IX_PatientPortalSessions_PatientId" ON "PatientPortalSessions" ("PatientId")""",

        """
        INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
        VALUES ('20260607000003_Phase3_PatientPortal', '10.0.8')
        ON CONFLICT ("MigrationId") DO NOTHING
        """,

        // Phase 4 — Lab order panels
        """
        CREATE TABLE IF NOT EXISTS "LabOrderPanels" (
            "Id" uuid NOT NULL,
            "Code" character varying(50) NOT NULL,
            "Name" character varying(200) NOT NULL,
            "Category" character varying(100),
            "IsActive" boolean NOT NULL DEFAULT true,
            "CreatedAtUtc" timestamp with time zone NOT NULL,
            CONSTRAINT "PK_LabOrderPanels" PRIMARY KEY ("Id")
        )
        """,
        """CREATE UNIQUE INDEX IF NOT EXISTS "IX_LabOrderPanels_Code" ON "LabOrderPanels" ("Code")""",

        // Phase 4 — Lab orders
        """
        CREATE TABLE IF NOT EXISTS "LabOrders" (
            "Id" uuid NOT NULL,
            "EncounterId" uuid NOT NULL,
            "PatientId" uuid NOT NULL,
            "OrderedByUserId" uuid NOT NULL,
            "PanelId" uuid NOT NULL,
            "Priority" character varying(20) NOT NULL DEFAULT 'Routine',
            "Status" character varying(20) NOT NULL DEFAULT 'Ordered',
            "OrderedAtUtc" timestamp with time zone NOT NULL,
            "CollectedAtUtc" timestamp with time zone,
            "ResultedAtUtc" timestamp with time zone,
            "Notes" character varying(1000),
            CONSTRAINT "PK_LabOrders" PRIMARY KEY ("Id"),
            CONSTRAINT "FK_LabOrders_ClinicalEncounters" FOREIGN KEY ("EncounterId") REFERENCES "ClinicalEncounters" ("Id") ON DELETE RESTRICT,
            CONSTRAINT "FK_LabOrders_Patients" FOREIGN KEY ("PatientId") REFERENCES "Patients" ("Id") ON DELETE RESTRICT,
            CONSTRAINT "FK_LabOrders_Users" FOREIGN KEY ("OrderedByUserId") REFERENCES "Users" ("Id") ON DELETE RESTRICT,
            CONSTRAINT "FK_LabOrders_Panels" FOREIGN KEY ("PanelId") REFERENCES "LabOrderPanels" ("Id") ON DELETE RESTRICT
        )
        """,
        """CREATE INDEX IF NOT EXISTS "IX_LabOrders_PatientId" ON "LabOrders" ("PatientId")""",
        """CREATE INDEX IF NOT EXISTS "IX_LabOrders_EncounterId" ON "LabOrders" ("EncounterId")""",
        """CREATE INDEX IF NOT EXISTS "IX_LabOrders_Status" ON "LabOrders" ("Status")""",

        // Phase 4 — Lab results
        """
        CREATE TABLE IF NOT EXISTS "LabResults" (
            "Id" uuid NOT NULL,
            "OrderId" uuid NOT NULL,
            "RecordedByUserId" uuid NOT NULL,
            "AnalyteName" character varying(200) NOT NULL,
            "Value" character varying(200) NOT NULL,
            "Unit" character varying(50),
            "ReferenceRange" character varying(100),
            "Flag" character varying(10) NOT NULL DEFAULT 'Normal',
            "RecordedAtUtc" timestamp with time zone NOT NULL,
            CONSTRAINT "PK_LabResults" PRIMARY KEY ("Id"),
            CONSTRAINT "FK_LabResults_LabOrders" FOREIGN KEY ("OrderId") REFERENCES "LabOrders" ("Id") ON DELETE CASCADE,
            CONSTRAINT "FK_LabResults_Users" FOREIGN KEY ("RecordedByUserId") REFERENCES "Users" ("Id") ON DELETE RESTRICT
        )
        """,
        """CREATE INDEX IF NOT EXISTS "IX_LabResults_OrderId" ON "LabResults" ("OrderId")""",

        // Phase 4 — Imaging requests
        """
        CREATE TABLE IF NOT EXISTS "ImagingRequests" (
            "Id" uuid NOT NULL,
            "EncounterId" uuid NOT NULL,
            "PatientId" uuid NOT NULL,
            "RequestedByUserId" uuid NOT NULL,
            "Modality" character varying(50) NOT NULL,
            "BodyPart" character varying(100),
            "ClinicalIndication" character varying(1000),
            "Priority" character varying(20) NOT NULL DEFAULT 'Routine',
            "Status" character varying(30) NOT NULL DEFAULT 'Requested',
            "RequestedAtUtc" timestamp with time zone NOT NULL,
            CONSTRAINT "PK_ImagingRequests" PRIMARY KEY ("Id"),
            CONSTRAINT "FK_ImagingRequests_ClinicalEncounters" FOREIGN KEY ("EncounterId") REFERENCES "ClinicalEncounters" ("Id") ON DELETE RESTRICT,
            CONSTRAINT "FK_ImagingRequests_Patients" FOREIGN KEY ("PatientId") REFERENCES "Patients" ("Id") ON DELETE RESTRICT,
            CONSTRAINT "FK_ImagingRequests_Users" FOREIGN KEY ("RequestedByUserId") REFERENCES "Users" ("Id") ON DELETE RESTRICT
        )
        """,
        """CREATE INDEX IF NOT EXISTS "IX_ImagingRequests_PatientId" ON "ImagingRequests" ("PatientId")""",
        """CREATE INDEX IF NOT EXISTS "IX_ImagingRequests_EncounterId" ON "ImagingRequests" ("EncounterId")""",
        """CREATE INDEX IF NOT EXISTS "IX_ImagingRequests_Status" ON "ImagingRequests" ("Status")""",

        // Phase 4 — Imaging reports (1:1 with ImagingRequests)
        """
        CREATE TABLE IF NOT EXISTS "ImagingReports" (
            "Id" uuid NOT NULL,
            "RequestId" uuid NOT NULL,
            "RadiologyUserId" uuid NOT NULL,
            "ReportText" character varying(8000) NOT NULL,
            "Impression" character varying(2000),
            "ReportedAtUtc" timestamp with time zone NOT NULL,
            "AttachmentPath" character varying(500),
            CONSTRAINT "PK_ImagingReports" PRIMARY KEY ("Id"),
            CONSTRAINT "FK_ImagingReports_ImagingRequests" FOREIGN KEY ("RequestId") REFERENCES "ImagingRequests" ("Id") ON DELETE CASCADE,
            CONSTRAINT "FK_ImagingReports_Users" FOREIGN KEY ("RadiologyUserId") REFERENCES "Users" ("Id") ON DELETE RESTRICT
        )
        """,
        """CREATE UNIQUE INDEX IF NOT EXISTS "IX_ImagingReports_RequestId" ON "ImagingReports" ("RequestId")""",

        """
        INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
        VALUES ('20260607000004_Phase4_LabImaging', '10.0.8')
        ON CONFLICT ("MigrationId") DO NOTHING
        """,

        // ── Phase 5: Billing & Insurance ─────────────────────────────────────
        """
        CREATE TABLE IF NOT EXISTS "ChargeItems" (
            "Id" uuid NOT NULL,
            "Code" character varying(50) NOT NULL,
            "Description" character varying(500) NOT NULL,
            "Category" character varying(100),
            "UnitPrice" numeric(18,2) NOT NULL DEFAULT 0,
            "IsActive" boolean NOT NULL DEFAULT true,
            "CreatedAtUtc" timestamp with time zone NOT NULL,
            CONSTRAINT "PK_ChargeItems" PRIMARY KEY ("Id")
        )
        """,
        """CREATE UNIQUE INDEX IF NOT EXISTS "IX_ChargeItems_Code" ON "ChargeItems" ("Code")""",

        """
        CREATE TABLE IF NOT EXISTS "InsuranceProviders" (
            "Id" uuid NOT NULL,
            "Name" character varying(200) NOT NULL,
            "ContactPhone" character varying(30),
            "ContactEmail" character varying(256),
            "Address" character varying(500),
            "IsActive" boolean NOT NULL DEFAULT true,
            "CreatedAtUtc" timestamp with time zone NOT NULL,
            CONSTRAINT "PK_InsuranceProviders" PRIMARY KEY ("Id")
        )
        """,

        """
        CREATE TABLE IF NOT EXISTS "PatientInsurances" (
            "Id" uuid NOT NULL,
            "PatientId" uuid NOT NULL,
            "ProviderId" uuid NOT NULL,
            "PolicyNumber" character varying(100) NOT NULL,
            "GroupNumber" character varying(100),
            "IsPrimary" boolean NOT NULL DEFAULT false,
            "ExpiresAt" timestamp with time zone,
            "IsActive" boolean NOT NULL DEFAULT true,
            "CreatedAtUtc" timestamp with time zone NOT NULL,
            CONSTRAINT "PK_PatientInsurances" PRIMARY KEY ("Id"),
            CONSTRAINT "FK_PatientInsurances_Patients" FOREIGN KEY ("PatientId") REFERENCES "Patients" ("Id") ON DELETE RESTRICT,
            CONSTRAINT "FK_PatientInsurances_InsuranceProviders" FOREIGN KEY ("ProviderId") REFERENCES "InsuranceProviders" ("Id") ON DELETE RESTRICT
        )
        """,
        """CREATE INDEX IF NOT EXISTS "IX_PatientInsurances_PatientId" ON "PatientInsurances" ("PatientId")""",

        """
        CREATE TABLE IF NOT EXISTS "Invoices" (
            "Id" uuid NOT NULL,
            "InvoiceNumber" character varying(50) NOT NULL,
            "PatientId" uuid NOT NULL,
            "EncounterId" uuid,
            "CreatedByUserId" uuid NOT NULL,
            "Status" character varying(30) NOT NULL DEFAULT 'Draft',
            "SubtotalAmount" numeric(18,2) NOT NULL DEFAULT 0,
            "DiscountAmount" numeric(18,2) NOT NULL DEFAULT 0,
            "TaxAmount" numeric(18,2) NOT NULL DEFAULT 0,
            "TotalAmount" numeric(18,2) NOT NULL DEFAULT 0,
            "PaidAmount" numeric(18,2) NOT NULL DEFAULT 0,
            "DueDate" timestamp with time zone,
            "Notes" character varying(1000),
            "CreatedAtUtc" timestamp with time zone NOT NULL,
            "UpdatedAtUtc" timestamp with time zone NOT NULL,
            CONSTRAINT "PK_Invoices" PRIMARY KEY ("Id"),
            CONSTRAINT "FK_Invoices_Patients" FOREIGN KEY ("PatientId") REFERENCES "Patients" ("Id") ON DELETE RESTRICT,
            CONSTRAINT "FK_Invoices_ClinicalEncounters" FOREIGN KEY ("EncounterId") REFERENCES "ClinicalEncounters" ("Id") ON DELETE SET NULL,
            CONSTRAINT "FK_Invoices_Users" FOREIGN KEY ("CreatedByUserId") REFERENCES "Users" ("Id") ON DELETE RESTRICT
        )
        """,
        """CREATE UNIQUE INDEX IF NOT EXISTS "IX_Invoices_InvoiceNumber" ON "Invoices" ("InvoiceNumber")""",
        """CREATE INDEX IF NOT EXISTS "IX_Invoices_PatientId" ON "Invoices" ("PatientId")""",
        """CREATE INDEX IF NOT EXISTS "IX_Invoices_Status" ON "Invoices" ("Status")""",

        """
        CREATE TABLE IF NOT EXISTS "InvoiceLineItems" (
            "Id" uuid NOT NULL,
            "InvoiceId" uuid NOT NULL,
            "ChargeItemId" uuid NOT NULL,
            "Description" character varying(500) NOT NULL,
            "Quantity" integer NOT NULL DEFAULT 1,
            "UnitPrice" numeric(18,2) NOT NULL,
            "TotalPrice" numeric(18,2) NOT NULL,
            CONSTRAINT "PK_InvoiceLineItems" PRIMARY KEY ("Id"),
            CONSTRAINT "FK_InvoiceLineItems_Invoices" FOREIGN KEY ("InvoiceId") REFERENCES "Invoices" ("Id") ON DELETE CASCADE,
            CONSTRAINT "FK_InvoiceLineItems_ChargeItems" FOREIGN KEY ("ChargeItemId") REFERENCES "ChargeItems" ("Id") ON DELETE RESTRICT
        )
        """,
        """CREATE INDEX IF NOT EXISTS "IX_InvoiceLineItems_InvoiceId" ON "InvoiceLineItems" ("InvoiceId")""",

        """
        CREATE TABLE IF NOT EXISTS "Payments" (
            "Id" uuid NOT NULL,
            "InvoiceId" uuid NOT NULL,
            "RecordedByUserId" uuid NOT NULL,
            "Amount" numeric(18,2) NOT NULL,
            "Method" character varying(30) NOT NULL,
            "ReferenceNumber" character varying(100),
            "Notes" character varying(500),
            "PaidAtUtc" timestamp with time zone NOT NULL,
            CONSTRAINT "PK_Payments" PRIMARY KEY ("Id"),
            CONSTRAINT "FK_Payments_Invoices" FOREIGN KEY ("InvoiceId") REFERENCES "Invoices" ("Id") ON DELETE RESTRICT,
            CONSTRAINT "FK_Payments_Users" FOREIGN KEY ("RecordedByUserId") REFERENCES "Users" ("Id") ON DELETE RESTRICT
        )
        """,
        """CREATE INDEX IF NOT EXISTS "IX_Payments_InvoiceId" ON "Payments" ("InvoiceId")""",

        """
        CREATE TABLE IF NOT EXISTS "InsuranceClaims" (
            "Id" uuid NOT NULL,
            "InvoiceId" uuid NOT NULL,
            "PatientInsuranceId" uuid NOT NULL,
            "ClaimNumber" character varying(100) NOT NULL,
            "Status" character varying(30) NOT NULL DEFAULT 'Pending',
            "ApprovedAmount" numeric(18,2),
            "RejectionReason" character varying(500),
            "SubmittedAtUtc" timestamp with time zone NOT NULL,
            "ResolvedAtUtc" timestamp with time zone,
            CONSTRAINT "PK_InsuranceClaims" PRIMARY KEY ("Id"),
            CONSTRAINT "FK_InsuranceClaims_Invoices" FOREIGN KEY ("InvoiceId") REFERENCES "Invoices" ("Id") ON DELETE RESTRICT,
            CONSTRAINT "FK_InsuranceClaims_PatientInsurances" FOREIGN KEY ("PatientInsuranceId") REFERENCES "PatientInsurances" ("Id") ON DELETE RESTRICT
        )
        """,
        """CREATE UNIQUE INDEX IF NOT EXISTS "IX_InsuranceClaims_ClaimNumber" ON "InsuranceClaims" ("ClaimNumber")""",
        """CREATE INDEX IF NOT EXISTS "IX_InsuranceClaims_InvoiceId" ON "InsuranceClaims" ("InvoiceId")""",

        """
        INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
        VALUES ('20260608000005_Phase5_Billing', '10.0.8')
        ON CONFLICT ("MigrationId") DO NOTHING
        """,

        // ── Phase 6: Bed Management ───────────────────────────────────────────
        """
        CREATE TABLE IF NOT EXISTS "Wards" (
            "Id" uuid NOT NULL,
            "Name" character varying(100) NOT NULL,
            "WardType" character varying(30) NOT NULL DEFAULT 'General',
            "TotalBeds" integer NOT NULL DEFAULT 0,
            "FloorNumber" integer NOT NULL DEFAULT 1,
            "IsActive" boolean NOT NULL DEFAULT true,
            "CreatedAtUtc" timestamp with time zone NOT NULL,
            CONSTRAINT "PK_Wards" PRIMARY KEY ("Id")
        )
        """,

        """
        CREATE TABLE IF NOT EXISTS "Beds" (
            "Id" uuid NOT NULL,
            "WardId" uuid NOT NULL,
            "BedNumber" character varying(20) NOT NULL,
            "BedType" character varying(30) NOT NULL DEFAULT 'Standard',
            "Status" character varying(30) NOT NULL DEFAULT 'Available',
            "IsActive" boolean NOT NULL DEFAULT true,
            "CreatedAtUtc" timestamp with time zone NOT NULL,
            CONSTRAINT "PK_Beds" PRIMARY KEY ("Id"),
            CONSTRAINT "FK_Beds_Wards" FOREIGN KEY ("WardId") REFERENCES "Wards" ("Id") ON DELETE CASCADE
        )
        """,
        """CREATE UNIQUE INDEX IF NOT EXISTS "IX_Beds_WardId_BedNumber" ON "Beds" ("WardId", "BedNumber")""",

        """
        CREATE TABLE IF NOT EXISTS "BedAllocations" (
            "Id" uuid NOT NULL,
            "BedId" uuid NOT NULL,
            "PatientId" uuid NOT NULL,
            "EncounterId" uuid,
            "AdmittedByUserId" uuid NOT NULL,
            "AdmittedAtUtc" timestamp with time zone NOT NULL,
            "DischargedAtUtc" timestamp with time zone,
            "DischargeReason" character varying(500),
            "DischargedByUserId" uuid,
            "TransferredToBedId" uuid,
            CONSTRAINT "PK_BedAllocations" PRIMARY KEY ("Id"),
            CONSTRAINT "FK_BedAllocations_Beds" FOREIGN KEY ("BedId") REFERENCES "Beds" ("Id") ON DELETE RESTRICT,
            CONSTRAINT "FK_BedAllocations_Patients" FOREIGN KEY ("PatientId") REFERENCES "Patients" ("Id") ON DELETE RESTRICT,
            CONSTRAINT "FK_BedAllocations_ClinicalEncounters" FOREIGN KEY ("EncounterId") REFERENCES "ClinicalEncounters" ("Id") ON DELETE SET NULL,
            CONSTRAINT "FK_BedAllocations_AdmittedByUser" FOREIGN KEY ("AdmittedByUserId") REFERENCES "Users" ("Id") ON DELETE RESTRICT,
            CONSTRAINT "FK_BedAllocations_DischargedByUser" FOREIGN KEY ("DischargedByUserId") REFERENCES "Users" ("Id") ON DELETE RESTRICT,
            CONSTRAINT "FK_BedAllocations_TransferredToBed" FOREIGN KEY ("TransferredToBedId") REFERENCES "Beds" ("Id") ON DELETE SET NULL
        )
        """,
        """CREATE UNIQUE INDEX IF NOT EXISTS "IX_BedAllocations_BedId_Active" ON "BedAllocations" ("BedId") WHERE "DischargedAtUtc" IS NULL""",
        """CREATE INDEX IF NOT EXISTS "IX_BedAllocations_PatientId" ON "BedAllocations" ("PatientId")""",
        """CREATE INDEX IF NOT EXISTS "IX_BedAllocations_BedId" ON "BedAllocations" ("BedId")""",

        """
        INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
        VALUES ('20260608000006_Phase6_BedManagement', '10.0.8')
        ON CONFLICT ("MigrationId") DO NOTHING
        """,

        // ── Phase 7: Inventory Management ────────────────────────────────────
        """
        CREATE TABLE IF NOT EXISTS "InventoryCategories" (
            "Id" uuid NOT NULL,
            "Name" character varying(100) NOT NULL,
            "Description" character varying(500),
            "IsActive" boolean NOT NULL DEFAULT true,
            "CreatedAtUtc" timestamp with time zone NOT NULL,
            CONSTRAINT "PK_InventoryCategories" PRIMARY KEY ("Id")
        )
        """,
        """CREATE UNIQUE INDEX IF NOT EXISTS "IX_InventoryCategories_Name" ON "InventoryCategories" ("Name")""",

        """
        CREATE TABLE IF NOT EXISTS "InventoryItems" (
            "Id" uuid NOT NULL,
            "CategoryId" uuid NOT NULL,
            "Code" character varying(50) NOT NULL,
            "Name" character varying(200) NOT NULL,
            "Description" character varying(500),
            "Unit" character varying(50) NOT NULL DEFAULT 'Unit',
            "ReorderLevel" integer NOT NULL DEFAULT 0,
            "CurrentStock" integer NOT NULL DEFAULT 0,
            "UnitCost" numeric(18,2) NOT NULL DEFAULT 0,
            "IsActive" boolean NOT NULL DEFAULT true,
            "CreatedAtUtc" timestamp with time zone NOT NULL,
            CONSTRAINT "PK_InventoryItems" PRIMARY KEY ("Id"),
            CONSTRAINT "FK_InventoryItems_InventoryCategories" FOREIGN KEY ("CategoryId") REFERENCES "InventoryCategories" ("Id") ON DELETE RESTRICT
        )
        """,
        """CREATE UNIQUE INDEX IF NOT EXISTS "IX_InventoryItems_Code" ON "InventoryItems" ("Code")""",
        """CREATE INDEX IF NOT EXISTS "IX_InventoryItems_CategoryId" ON "InventoryItems" ("CategoryId")""",

        """
        CREATE TABLE IF NOT EXISTS "StockTransactions" (
            "Id" uuid NOT NULL,
            "ItemId" uuid NOT NULL,
            "Type" character varying(20) NOT NULL DEFAULT 'In',
            "Quantity" integer NOT NULL,
            "StockAfter" integer NOT NULL,
            "ReferenceType" character varying(50),
            "ReferenceId" uuid,
            "Notes" character varying(500),
            "PerformedByUserId" uuid NOT NULL,
            "TransactedAtUtc" timestamp with time zone NOT NULL,
            CONSTRAINT "PK_StockTransactions" PRIMARY KEY ("Id"),
            CONSTRAINT "FK_StockTransactions_InventoryItems" FOREIGN KEY ("ItemId") REFERENCES "InventoryItems" ("Id") ON DELETE CASCADE,
            CONSTRAINT "FK_StockTransactions_Users" FOREIGN KEY ("PerformedByUserId") REFERENCES "Users" ("Id") ON DELETE RESTRICT
        )
        """,
        """CREATE INDEX IF NOT EXISTS "IX_StockTransactions_ItemId" ON "StockTransactions" ("ItemId")""",
        """CREATE INDEX IF NOT EXISTS "IX_StockTransactions_TransactedAtUtc" ON "StockTransactions" ("TransactedAtUtc")""",

        """
        INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
        VALUES ('20260608000007_Phase7_Inventory', '10.0.8')
        ON CONFLICT ("MigrationId") DO NOTHING
        """,

        // ── Phase 9: Supply Chain ─────────────────────────────────────────────
        """
        CREATE TABLE IF NOT EXISTS "Suppliers" (
            "Id" uuid NOT NULL,
            "Name" character varying(200) NOT NULL,
            "ContactName" character varying(100),
            "ContactPhone" character varying(30),
            "ContactEmail" character varying(256),
            "Address" character varying(500),
            "IsActive" boolean NOT NULL DEFAULT true,
            "CreatedAtUtc" timestamp with time zone NOT NULL,
            CONSTRAINT "PK_Suppliers" PRIMARY KEY ("Id")
        )
        """,

        """
        CREATE TABLE IF NOT EXISTS "PurchaseOrders" (
            "Id" uuid NOT NULL,
            "SupplierId" uuid NOT NULL,
            "OrderedByUserId" uuid NOT NULL,
            "OrderNumber" character varying(30) NOT NULL,
            "Status" character varying(30) NOT NULL DEFAULT 'Draft',
            "Notes" character varying(1000),
            "ExpectedDeliveryDate" timestamp with time zone,
            "OrderedAtUtc" timestamp with time zone NOT NULL,
            "CreatedAtUtc" timestamp with time zone NOT NULL,
            "UpdatedAtUtc" timestamp with time zone NOT NULL,
            CONSTRAINT "PK_PurchaseOrders" PRIMARY KEY ("Id"),
            CONSTRAINT "FK_PurchaseOrders_Suppliers" FOREIGN KEY ("SupplierId") REFERENCES "Suppliers" ("Id") ON DELETE RESTRICT,
            CONSTRAINT "FK_PurchaseOrders_Users" FOREIGN KEY ("OrderedByUserId") REFERENCES "Users" ("Id") ON DELETE RESTRICT
        )
        """,
        """CREATE UNIQUE INDEX IF NOT EXISTS "IX_PurchaseOrders_OrderNumber" ON "PurchaseOrders" ("OrderNumber")""",
        """CREATE INDEX IF NOT EXISTS "IX_PurchaseOrders_SupplierId" ON "PurchaseOrders" ("SupplierId")""",
        """CREATE INDEX IF NOT EXISTS "IX_PurchaseOrders_Status" ON "PurchaseOrders" ("Status")""",

        """
        CREATE TABLE IF NOT EXISTS "PurchaseOrderLines" (
            "Id" uuid NOT NULL,
            "PurchaseOrderId" uuid NOT NULL,
            "InventoryItemId" uuid NOT NULL,
            "Description" character varying(500) NOT NULL,
            "QuantityOrdered" integer NOT NULL DEFAULT 1,
            "QuantityReceived" integer NOT NULL DEFAULT 0,
            "UnitCost" numeric(18,2) NOT NULL DEFAULT 0,
            "TotalCost" numeric(18,2) NOT NULL DEFAULT 0,
            CONSTRAINT "PK_PurchaseOrderLines" PRIMARY KEY ("Id"),
            CONSTRAINT "FK_PurchaseOrderLines_PurchaseOrders" FOREIGN KEY ("PurchaseOrderId") REFERENCES "PurchaseOrders" ("Id") ON DELETE CASCADE,
            CONSTRAINT "FK_PurchaseOrderLines_InventoryItems" FOREIGN KEY ("InventoryItemId") REFERENCES "InventoryItems" ("Id") ON DELETE RESTRICT
        )
        """,
        """CREATE INDEX IF NOT EXISTS "IX_PurchaseOrderLines_PurchaseOrderId" ON "PurchaseOrderLines" ("PurchaseOrderId")""",

        """
        CREATE TABLE IF NOT EXISTS "GoodsReceipts" (
            "Id" uuid NOT NULL,
            "PurchaseOrderId" uuid NOT NULL,
            "ReceivedByUserId" uuid NOT NULL,
            "Notes" character varying(1000),
            "ReceivedAtUtc" timestamp with time zone NOT NULL,
            CONSTRAINT "PK_GoodsReceipts" PRIMARY KEY ("Id"),
            CONSTRAINT "FK_GoodsReceipts_PurchaseOrders" FOREIGN KEY ("PurchaseOrderId") REFERENCES "PurchaseOrders" ("Id") ON DELETE RESTRICT,
            CONSTRAINT "FK_GoodsReceipts_Users" FOREIGN KEY ("ReceivedByUserId") REFERENCES "Users" ("Id") ON DELETE RESTRICT
        )
        """,
        """CREATE INDEX IF NOT EXISTS "IX_GoodsReceipts_PurchaseOrderId" ON "GoodsReceipts" ("PurchaseOrderId")""",

        """
        CREATE TABLE IF NOT EXISTS "GoodsReceiptLines" (
            "Id" uuid NOT NULL,
            "GoodsReceiptId" uuid NOT NULL,
            "PurchaseOrderLineId" uuid NOT NULL,
            "QuantityReceived" integer NOT NULL DEFAULT 0,
            CONSTRAINT "PK_GoodsReceiptLines" PRIMARY KEY ("Id"),
            CONSTRAINT "FK_GoodsReceiptLines_GoodsReceipts" FOREIGN KEY ("GoodsReceiptId") REFERENCES "GoodsReceipts" ("Id") ON DELETE CASCADE,
            CONSTRAINT "FK_GoodsReceiptLines_PurchaseOrderLines" FOREIGN KEY ("PurchaseOrderLineId") REFERENCES "PurchaseOrderLines" ("Id") ON DELETE RESTRICT
        )
        """,
        """CREATE INDEX IF NOT EXISTS "IX_GoodsReceiptLines_GoodsReceiptId" ON "GoodsReceiptLines" ("GoodsReceiptId")""",

        """
        INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
        VALUES ('20260608000009_Phase9_SupplyChain', '10.0.8')
        ON CONFLICT ("MigrationId") DO NOTHING
        """,

        // ── Phase 10: Audit Persistence ──────────────────────────────────────
        """
        CREATE TABLE IF NOT EXISTS "AuditEntries" (
            "Id" uuid NOT NULL,
            "EntityType" character varying(100) NOT NULL,
            "EntityId" uuid,
            "Action" character varying(50) NOT NULL,
            "Details" character varying(2000),
            "PerformedByUserId" uuid,
            "PerformedByUsername" character varying(100),
            "IpAddress" character varying(45),
            "PerformedAtUtc" timestamp with time zone NOT NULL,
            CONSTRAINT "PK_AuditEntries" PRIMARY KEY ("Id")
        )
        """,
        """CREATE INDEX IF NOT EXISTS "IX_AuditEntries_EntityType" ON "AuditEntries" ("EntityType")""",
        """CREATE INDEX IF NOT EXISTS "IX_AuditEntries_PerformedByUserId" ON "AuditEntries" ("PerformedByUserId")""",
        """CREATE INDEX IF NOT EXISTS "IX_AuditEntries_PerformedAtUtc" ON "AuditEntries" ("PerformedAtUtc" DESC)""",

        """
        INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
        VALUES ('20260608000010_Phase10_AuditPersistence', '10.0.8')
        ON CONFLICT ("MigrationId") DO NOTHING
        """
    ];
}
