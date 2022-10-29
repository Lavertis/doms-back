using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DoctorsOffice.Infrastructure.Migrations
{
    public partial class SeededUsersHaveConfirmedEmail : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("4facc425-b1ef-416a-979f-56da897448c5"),
                column: "EmailConfirmed",
                value: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"),
                column: "EmailConfirmed",
                value: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("f2f0ccba-ce3c-4ce4-8167-b79d88117c05"),
                column: "EmailConfirmed",
                value: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("4facc425-b1ef-416a-979f-56da897448c5"),
                column: "EmailConfirmed",
                value: false);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"),
                column: "EmailConfirmed",
                value: false);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("f2f0ccba-ce3c-4ce4-8167-b79d88117c05"),
                column: "EmailConfirmed",
                value: false);
        }
    }
}
