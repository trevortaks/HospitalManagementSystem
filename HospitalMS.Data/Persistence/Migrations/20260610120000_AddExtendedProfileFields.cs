using Microsoft.EntityFrameworkCore.Migrations;

namespace HospitalMS.Data.Persistence.Migrations;

[Migration("20260610120000_AddExtendedProfileFields")]
public partial class AddExtendedProfileFields : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Patient — new address fields
        migrationBuilder.AddColumn<string>(
            name: "AddressLine2",
            table: "Patients",
            type: "character varying(200)",
            maxLength: 200,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "State",
            table: "Patients",
            type: "character varying(100)",
            maxLength: 100,
            nullable: true);

        // Patient — emergency contact extensions
        migrationBuilder.AddColumn<string>(
            name: "EmergencyContactRelationship",
            table: "Patients",
            type: "character varying(100)",
            maxLength: 100,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "EmergencyContactEmail",
            table: "Patients",
            type: "character varying(256)",
            maxLength: 256,
            nullable: true);

        // Patient — medical background
        migrationBuilder.AddColumn<string>(
            name: "Allergies",
            table: "Patients",
            type: "character varying(2000)",
            maxLength: 2000,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "ChronicConditions",
            table: "Patients",
            type: "character varying(2000)",
            maxLength: 2000,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "Notes",
            table: "Patients",
            type: "character varying(4000)",
            maxLength: 4000,
            nullable: true);

        // User — personal details
        migrationBuilder.AddColumn<string>(
            name: "FirstName",
            table: "Users",
            type: "character varying(100)",
            maxLength: 100,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "LastName",
            table: "Users",
            type: "character varying(100)",
            maxLength: 100,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "PhoneNumber",
            table: "Users",
            type: "character varying(20)",
            maxLength: 20,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "AddressLine1",
            table: "Users",
            type: "character varying(200)",
            maxLength: 200,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "City",
            table: "Users",
            type: "character varying(100)",
            maxLength: 100,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "PostalCode",
            table: "Users",
            type: "character varying(20)",
            maxLength: 20,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "Country",
            table: "Users",
            type: "character varying(100)",
            maxLength: 100,
            nullable: true);

        // User — professional details
        migrationBuilder.AddColumn<string>(
            name: "Specialization",
            table: "Users",
            type: "character varying(200)",
            maxLength: 200,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "LicenseNumber",
            table: "Users",
            type: "character varying(100)",
            maxLength: 100,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "Bio",
            table: "Users",
            type: "character varying(1000)",
            maxLength: 1000,
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "AddressLine2", table: "Patients");
        migrationBuilder.DropColumn(name: "State", table: "Patients");
        migrationBuilder.DropColumn(name: "EmergencyContactRelationship", table: "Patients");
        migrationBuilder.DropColumn(name: "EmergencyContactEmail", table: "Patients");
        migrationBuilder.DropColumn(name: "Allergies", table: "Patients");
        migrationBuilder.DropColumn(name: "ChronicConditions", table: "Patients");
        migrationBuilder.DropColumn(name: "Notes", table: "Patients");

        migrationBuilder.DropColumn(name: "FirstName", table: "Users");
        migrationBuilder.DropColumn(name: "LastName", table: "Users");
        migrationBuilder.DropColumn(name: "PhoneNumber", table: "Users");
        migrationBuilder.DropColumn(name: "AddressLine1", table: "Users");
        migrationBuilder.DropColumn(name: "City", table: "Users");
        migrationBuilder.DropColumn(name: "PostalCode", table: "Users");
        migrationBuilder.DropColumn(name: "Country", table: "Users");
        migrationBuilder.DropColumn(name: "Specialization", table: "Users");
        migrationBuilder.DropColumn(name: "LicenseNumber", table: "Users");
        migrationBuilder.DropColumn(name: "Bio", table: "Users");
    }
}
