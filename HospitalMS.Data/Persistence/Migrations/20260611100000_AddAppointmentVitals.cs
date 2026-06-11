using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HospitalMS.Data.Persistence.Migrations;

[Migration("20260611100000_AddAppointmentVitals")]
public partial class AddAppointmentVitals : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "AppointmentVitals",
            columns: table => new
            {
                Id = table.Column<Guid>(nullable: false),
                AppointmentId = table.Column<Guid>(nullable: false),
                RecordedByUserId = table.Column<Guid>(nullable: false),
                RecordedAtUtc = table.Column<DateTime>(nullable: false),
                HeightCm = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                WeightKg = table.Column<decimal>(type: "numeric(6,2)", nullable: true),
                TemperatureCelsius = table.Column<decimal>(type: "numeric(4,1)", nullable: true),
                BloodPressureSystolic = table.Column<int>(nullable: true),
                BloodPressureDiastolic = table.Column<int>(nullable: true),
                HeartRateBpm = table.Column<int>(nullable: true),
                RespiratoryRate = table.Column<int>(nullable: true),
                OxygenSaturationPct = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                Notes = table.Column<string>(maxLength: 1000, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AppointmentVitals", x => x.Id);
                table.ForeignKey(
                    name: "FK_AppointmentVitals_Appointments_AppointmentId",
                    column: x => x.AppointmentId,
                    principalTable: "Appointments",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_AppointmentVitals_Users_RecordedByUserId",
                    column: x => x.RecordedByUserId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_AppointmentVitals_AppointmentId",
            table: "AppointmentVitals",
            column: "AppointmentId",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "AppointmentVitals");
    }
}
