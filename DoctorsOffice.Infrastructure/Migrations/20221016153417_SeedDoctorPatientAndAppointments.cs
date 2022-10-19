using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DoctorsOffice.Infrastructure.Migrations
{
    public partial class SeedDoctorPatientAndAppointments : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "Email", "EmailConfirmed", "FirstName", "LastName", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[,]
                {
                    { new Guid("4facc425-b1ef-416a-979f-56da897448c5"), 0, "4facc425-b1ef-416a-979f-56da897448c5", null, false, "Patient", "Patient", null, "PATIENT", "AL9EaDGX0cdo1q6ldEn3SDtSYoYHcRpcEBXmM4TUfF+hOIT06L6ZfvndiURMFQEphw==", null, false, "4facc425-b1ef-416a-979f-56da897448c5", false, "patient" },
                    { new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), 0, "c8934fff-2f5a-4198-893f-26023d8f4107", null, false, "Doctor", "Doctor", null, "DOCTOR", "AMbTv46BLUYaRTuuF5U53eDGMBRw4T7wQwaxSxTrM4mPB87g87fP+FW4n+ecgCXCdg==", null, false, "c8934fff-2f5a-4198-893f-26023d8f4107", false, "doctor" }
                });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[,]
                {
                    { new Guid("d4349d0c-d18c-4324-be02-254ad1208004"), new Guid("4facc425-b1ef-416a-979f-56da897448c5") },
                    { new Guid("80389a16-fbd0-4db1-b655-05a29d202a75"), new Guid("c8934fff-2f5a-4198-893f-26023d8f4107") }
                });

            migrationBuilder.InsertData(
                table: "Doctors",
                columns: new[] { "Id", "CreatedAt", "UpdatedAt" },
                values: new object[] { new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.InsertData(
                table: "Patients",
                columns: new[] { "Id", "Address", "CreatedAt", "DateOfBirth", "FirstName", "LastName", "NationalId", "UpdatedAt" },
                values: new object[] { new Guid("4facc425-b1ef-416a-979f-56da897448c5"), "7865 Greenview St. Randallstown, MD 21133", new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2000, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Patient", "Patient", "04233040549", new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.InsertData(
                table: "Appointments",
                columns: new[] { "Id", "CreatedAt", "Date", "Description", "Diagnosis", "DoctorId", "Interview", "PatientId", "Recommendations", "StatusId", "TypeId", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("2cf674a8-9311-4515-a6bb-8d8094ade09c"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2022, 11, 5, 0, 0, 0, 0, DateTimeKind.Utc), "4000000", null, new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), null, new Guid("4facc425-b1ef-416a-979f-56da897448c5"), null, new Guid("b7a08d2e-116d-42e3-9ec5-1aa0636d116c"), new Guid("e58cabc9-e259-42ff-a2a1-0e8d39bb900e"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("4f319bc2-a6d9-4a52-9357-0772d0edd639"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2022, 11, 3, 0, 0, 0, 0, DateTimeKind.Utc), "2000000", null, new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), null, new Guid("4facc425-b1ef-416a-979f-56da897448c5"), null, new Guid("b7a08d2e-116d-42e3-9ec5-1aa0636d116c"), new Guid("e58cabc9-e259-42ff-a2a1-0e8d39bb900e"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("56a26dea-caf2-4a4b-a013-ed9e776d25dc"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2022, 11, 1, 0, 0, 0, 0, DateTimeKind.Utc), "0", null, new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), null, new Guid("4facc425-b1ef-416a-979f-56da897448c5"), null, new Guid("b7a08d2e-116d-42e3-9ec5-1aa0636d116c"), new Guid("e58cabc9-e259-42ff-a2a1-0e8d39bb900e"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("9e1cf297-b90f-436a-8cd3-8ca95276872f"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2022, 11, 2, 0, 0, 0, 0, DateTimeKind.Utc), "1000000", null, new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), null, new Guid("4facc425-b1ef-416a-979f-56da897448c5"), null, new Guid("b7a08d2e-116d-42e3-9ec5-1aa0636d116c"), new Guid("e58cabc9-e259-42ff-a2a1-0e8d39bb900e"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("f32e24af-265d-4748-be59-769db539cb07"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2022, 11, 4, 0, 0, 0, 0, DateTimeKind.Utc), "3000000", null, new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), null, new Guid("4facc425-b1ef-416a-979f-56da897448c5"), null, new Guid("b7a08d2e-116d-42e3-9ec5-1aa0636d116c"), new Guid("e58cabc9-e259-42ff-a2a1-0e8d39bb900e"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc) }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Appointments",
                keyColumn: "Id",
                keyValue: new Guid("2cf674a8-9311-4515-a6bb-8d8094ade09c"));

            migrationBuilder.DeleteData(
                table: "Appointments",
                keyColumn: "Id",
                keyValue: new Guid("4f319bc2-a6d9-4a52-9357-0772d0edd639"));

            migrationBuilder.DeleteData(
                table: "Appointments",
                keyColumn: "Id",
                keyValue: new Guid("56a26dea-caf2-4a4b-a013-ed9e776d25dc"));

            migrationBuilder.DeleteData(
                table: "Appointments",
                keyColumn: "Id",
                keyValue: new Guid("9e1cf297-b90f-436a-8cd3-8ca95276872f"));

            migrationBuilder.DeleteData(
                table: "Appointments",
                keyColumn: "Id",
                keyValue: new Guid("f32e24af-265d-4748-be59-769db539cb07"));

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { new Guid("d4349d0c-d18c-4324-be02-254ad1208004"), new Guid("4facc425-b1ef-416a-979f-56da897448c5") });

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { new Guid("80389a16-fbd0-4db1-b655-05a29d202a75"), new Guid("c8934fff-2f5a-4198-893f-26023d8f4107") });

            migrationBuilder.DeleteData(
                table: "Doctors",
                keyColumn: "Id",
                keyValue: new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"));

            migrationBuilder.DeleteData(
                table: "Patients",
                keyColumn: "Id",
                keyValue: new Guid("4facc425-b1ef-416a-979f-56da897448c5"));

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("4facc425-b1ef-416a-979f-56da897448c5"));

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"));
        }
    }
}
