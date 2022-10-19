using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DoctorsOffice.Infrastructure.Migrations
{
    public partial class RemoveNamesFromPatient : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "Patients");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "Patients",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "Patients",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Patients",
                keyColumn: "Id",
                keyValue: new Guid("4facc425-b1ef-416a-979f-56da897448c5"),
                columns: new[] { "FirstName", "LastName" },
                values: new object[] { "Patient", "Patient" });
        }
    }
}
