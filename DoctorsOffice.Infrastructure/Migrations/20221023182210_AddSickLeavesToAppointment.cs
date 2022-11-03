using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DoctorsOffice.Infrastructure.Migrations
{
    public partial class AddSickLeavesToAppointment : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AppointmentId",
                table: "SickLeaves",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SickLeaves_AppointmentId",
                table: "SickLeaves",
                column: "AppointmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_SickLeaves_Appointments_AppointmentId",
                table: "SickLeaves",
                column: "AppointmentId",
                principalTable: "Appointments",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SickLeaves_Appointments_AppointmentId",
                table: "SickLeaves");

            migrationBuilder.DropIndex(
                name: "IX_SickLeaves_AppointmentId",
                table: "SickLeaves");

            migrationBuilder.DropColumn(
                name: "AppointmentId",
                table: "SickLeaves");
        }
    }
}
