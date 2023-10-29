using Microsoft.EntityFrameworkCore.Migrations;

namespace Student_attendence.Migrations
{
    public partial class updated_date_atreg : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AttendenceDate",
                table: "UnRegisteredAttendenceData",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FatherName",
                table: "UnRegisteredAttendenceData",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AttendenceDate",
                table: "RegisteredAttendenceData",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AttendenceDate",
                table: "UnRegisteredAttendenceData");

            migrationBuilder.DropColumn(
                name: "FatherName",
                table: "UnRegisteredAttendenceData");

            migrationBuilder.DropColumn(
                name: "AttendenceDate",
                table: "RegisteredAttendenceData");
        }
    }
}
