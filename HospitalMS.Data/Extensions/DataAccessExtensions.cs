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
        """
    ];
}
