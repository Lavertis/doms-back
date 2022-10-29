using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DoctorsOffice.Infrastructure.Migrations
{
    public partial class ChangeAdminEmail : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("f2f0ccba-ce3c-4ce4-8167-b79d88117c05"),
                columns: new[] { "Email", "NormalizedEmail" },
                values: new object[] { "admin@doms.com", "ADMIN@DOMS.COM" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("f2f0ccba-ce3c-4ce4-8167-b79d88117c05"),
                columns: new[] { "Email", "NormalizedEmail" },
                values: new object[] { "admin@admin.com", "ADMIN@ADMIN.COM" });
        }
    }
}
