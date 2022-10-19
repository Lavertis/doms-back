using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DoctorsOffice.Infrastructure.Migrations
{
    public partial class AddEmailAndPhoneNumberToSeededUsers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("4facc425-b1ef-416a-979f-56da897448c5"),
                columns: new[] { "Email", "NormalizedEmail", "PhoneNumber" },
                values: new object[] { "patient@patient.com", "PATIENT@PATIENT.COM", "123456789" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"),
                columns: new[] { "Email", "NormalizedEmail", "PhoneNumber" },
                values: new object[] { "doctor@doctor.com", "DOCTOR@DOCTOR.COM", "123456789" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("4facc425-b1ef-416a-979f-56da897448c5"),
                columns: new[] { "Email", "NormalizedEmail", "PhoneNumber" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"),
                columns: new[] { "Email", "NormalizedEmail", "PhoneNumber" },
                values: new object[] { null, null, null });
        }
    }
}
