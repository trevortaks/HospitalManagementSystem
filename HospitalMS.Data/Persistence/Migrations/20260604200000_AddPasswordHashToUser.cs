using Microsoft.EntityFrameworkCore.Migrations;

namespace HospitalMS.Data.Persistence.Migrations;

[Migration("20260604200000_AddPasswordHashToUser")]
public partial class AddPasswordHashToUser : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "PasswordHash",
            table: "Users",
            type: "character varying(100)",
            maxLength: 100,
            nullable: false,
            defaultValue: "");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "PasswordHash",
            table: "Users");
    }
}
