using Microsoft.EntityFrameworkCore.Migrations;

namespace Student_attendence.Migrations
{
    public partial class updated_doi_gender : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "EmailId",
                table: "StudentMasterData",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Branch",
                table: "StudentMasterData",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FatherDOI",
                table: "StudentMasterData",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Gender",
                table: "StudentMasterData",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "IsSatsangi",
                table: "StudentMasterData",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Mohalla",
                table: "StudentMasterData",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MotherDOI",
                table: "StudentMasterData",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Gender",
                table: "AdminUserData",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FatherDOI",
                table: "StudentMasterData");

            migrationBuilder.DropColumn(
                name: "Gender",
                table: "StudentMasterData");

            migrationBuilder.DropColumn(
                name: "IsSatsangi",
                table: "StudentMasterData");

            migrationBuilder.DropColumn(
                name: "Mohalla",
                table: "StudentMasterData");

            migrationBuilder.DropColumn(
                name: "MotherDOI",
                table: "StudentMasterData");

            migrationBuilder.DropColumn(
                name: "Gender",
                table: "AdminUserData");

            migrationBuilder.AlterColumn<string>(
                name: "EmailId",
                table: "StudentMasterData",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(40)",
                oldMaxLength: 40,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Branch",
                table: "StudentMasterData",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(40)",
                oldMaxLength: 40,
                oldNullable: true);
        }
    }
}
