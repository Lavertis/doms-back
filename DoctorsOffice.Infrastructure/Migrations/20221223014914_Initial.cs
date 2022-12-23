using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DoctorsOffice.Infrastructure.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppointmentStatuses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppointmentStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppointmentTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    DurationMinutes = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppointmentTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FirstName = table.Column<string>(type: "text", nullable: false),
                    LastName = table.Column<string>(type: "text", nullable: false),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Admins",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Admins", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Admins_AspNetUsers_Id",
                        column: x => x.Id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    ProviderKey = table.Column<string>(type: "text", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Doctors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Doctors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Doctors_AspNetUsers_Id",
                        column: x => x.Id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Patients",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NationalId = table.Column<string>(type: "text", nullable: false),
                    Address = table.Column<string>(type: "text", nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Patients", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Patients_AspNetUsers_Id",
                        column: x => x.Id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Token = table.Column<string>(type: "text", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedByIp = table.Column<string>(type: "text", nullable: true),
                    RevokedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RevokedByIp = table.Column<string>(type: "text", nullable: true),
                    ReplacedByToken = table.Column<string>(type: "text", nullable: true),
                    ReasonRevoked = table.Column<string>(type: "text", nullable: true),
                    AppUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_AspNetUsers_AppUserId",
                        column: x => x.AppUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuickButtons",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    DoctorId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuickButtons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuickButtons_Doctors_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "Doctors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Timetables",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StartDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DoctorId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Timetables", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Timetables_Doctors_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "Doctors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Appointments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Interview = table.Column<string>(type: "text", nullable: true),
                    Diagnosis = table.Column<string>(type: "text", nullable: true),
                    Recommendations = table.Column<string>(type: "text", nullable: true),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    DoctorId = table.Column<Guid>(type: "uuid", nullable: false),
                    StatusId = table.Column<Guid>(type: "uuid", nullable: false),
                    TypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Appointments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Appointments_AppointmentStatuses_StatusId",
                        column: x => x.StatusId,
                        principalTable: "AppointmentStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Appointments_AppointmentTypes_TypeId",
                        column: x => x.TypeId,
                        principalTable: "AppointmentTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Appointments_Doctors_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "Doctors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Appointments_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Prescriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FulfillmentDeadline = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DoctorId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    AppointmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prescriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Prescriptions_Appointments_AppointmentId",
                        column: x => x.AppointmentId,
                        principalTable: "Appointments",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Prescriptions_Doctors_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "Doctors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Prescriptions_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SickLeaves",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    AppointmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    DoctorId = table.Column<Guid>(type: "uuid", nullable: false),
                    DateStart = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateEnd = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Diagnosis = table.Column<string>(type: "text", nullable: false),
                    Purpose = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SickLeaves", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SickLeaves_Appointments_AppointmentId",
                        column: x => x.AppointmentId,
                        principalTable: "Appointments",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SickLeaves_Doctors_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "Doctors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SickLeaves_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DrugItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Rxcui = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    Dosage = table.Column<string>(type: "text", nullable: false),
                    PrescriptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DrugItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DrugItems_Prescriptions_PrescriptionId",
                        column: x => x.PrescriptionId,
                        principalTable: "Prescriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "AppointmentStatuses",
                columns: new[] { "Id", "CreatedAt", "Name", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("1cf993e4-73f2-497f-ad38-bccb4b4d0eee"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "REJECTED", new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("5de8a7ba-fb65-464f-9583-181d20d44b1b"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "COMPLETED", new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("8445a2f4-97cd-45c9-921f-f649f85cc0be"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "ACCEPTED", new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("b7a08d2e-116d-42e3-9ec5-1aa0636d116c"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "PENDING", new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("ccbb0db5-1661-4f9b-9482-67280ebdb6b5"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "CANCELLED", new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                table: "AppointmentTypes",
                columns: new[] { "Id", "CreatedAt", "DurationMinutes", "Name", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("532ec4d6-a4ad-4ece-a0b5-9f03e1033bf5"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), 30, "Consultation", new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("e58cabc9-e259-42ff-a2a1-0e8d39bb900e"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), 60, "Checkup", new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { new Guid("6506ab69-c793-4d0a-87d4-6565e98523d4"), "6506ab69-c793-4d0a-87d4-6565e98523d4", "Admin", "ADMIN" },
                    { new Guid("80389a16-fbd0-4db1-b655-05a29d202a75"), "80389a16-fbd0-4db1-b655-05a29d202a75", "Doctor", "DOCTOR" },
                    { new Guid("d4349d0c-d18c-4324-be02-254ad1208004"), "d4349d0c-d18c-4324-be02-254ad1208004", "Patient", "PATIENT" }
                });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "Email", "EmailConfirmed", "FirstName", "LastName", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[,]
                {
                    { new Guid("4facc425-b1ef-416a-979f-56da897448c5"), 0, "4facc425-b1ef-416a-979f-56da897448c5", "patient@patient.com", true, "Patient", "Patient", "PATIENT@PATIENT.COM", "PATIENT", "AL9EaDGX0cdo1q6ldEn3SDtSYoYHcRpcEBXmM4TUfF+hOIT06L6ZfvndiURMFQEphw==", "123456789", false, "4facc425-b1ef-416a-979f-56da897448c5", false, "patient" },
                    { new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), 0, "c8934fff-2f5a-4198-893f-26023d8f4107", "doctor@doctor.com", true, "Doctor", "Doctor", "DOCTOR@DOCTOR.COM", "DOCTOR", "AMbTv46BLUYaRTuuF5U53eDGMBRw4T7wQwaxSxTrM4mPB87g87fP+FW4n+ecgCXCdg==", "123456789", false, "c8934fff-2f5a-4198-893f-26023d8f4107", false, "doctor" },
                    { new Guid("f2f0ccba-ce3c-4ce4-8167-b79d88117c05"), 0, "f2f0ccba-ce3c-4ce4-8167-b79d88117c05", "admin@doms.com", true, "Admin", "Admin", "ADMIN@DOMS.COM", "ADMIN", "ACwoXDy/z+O6bjrLgviDbsZ036YrMsYj/fMPviVIsW1welLPf0g9dCgRkUTW3JOSpA==", null, false, "f2f0ccba-ce3c-4ce4-8167-b79d88117c05", false, "admin" }
                });

            migrationBuilder.InsertData(
                table: "Admins",
                columns: new[] { "Id", "CreatedAt", "UpdatedAt" },
                values: new object[] { new Guid("f2f0ccba-ce3c-4ce4-8167-b79d88117c05"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[,]
                {
                    { new Guid("d4349d0c-d18c-4324-be02-254ad1208004"), new Guid("4facc425-b1ef-416a-979f-56da897448c5") },
                    { new Guid("80389a16-fbd0-4db1-b655-05a29d202a75"), new Guid("c8934fff-2f5a-4198-893f-26023d8f4107") },
                    { new Guid("6506ab69-c793-4d0a-87d4-6565e98523d4"), new Guid("f2f0ccba-ce3c-4ce4-8167-b79d88117c05") }
                });

            migrationBuilder.InsertData(
                table: "Doctors",
                columns: new[] { "Id", "CreatedAt", "UpdatedAt" },
                values: new object[] { new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.InsertData(
                table: "Patients",
                columns: new[] { "Id", "Address", "CreatedAt", "DateOfBirth", "NationalId", "UpdatedAt" },
                values: new object[] { new Guid("4facc425-b1ef-416a-979f-56da897448c5"), "7865 Greenview St. Randallstown, MD 21133", new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2000, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "04233040549", new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.InsertData(
                table: "Appointments",
                columns: new[] { "Id", "CreatedAt", "Date", "Description", "Diagnosis", "DoctorId", "Interview", "PatientId", "Recommendations", "StatusId", "TypeId", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("2cf674a8-9311-4515-a6bb-8d8094ade09c"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2022, 11, 11, 11, 0, 0, 0, DateTimeKind.Utc), "4000000", null, new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), null, new Guid("4facc425-b1ef-416a-979f-56da897448c5"), null, new Guid("5de8a7ba-fb65-464f-9583-181d20d44b1b"), new Guid("e58cabc9-e259-42ff-a2a1-0e8d39bb900e"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("4f319bc2-a6d9-4a52-9357-0772d0edd639"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2022, 11, 9, 9, 0, 0, 0, DateTimeKind.Utc), "2000000", null, new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), null, new Guid("4facc425-b1ef-416a-979f-56da897448c5"), null, new Guid("1cf993e4-73f2-497f-ad38-bccb4b4d0eee"), new Guid("e58cabc9-e259-42ff-a2a1-0e8d39bb900e"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("56a26dea-caf2-4a4b-a013-ed9e776d25dc"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2022, 11, 7, 7, 0, 0, 0, DateTimeKind.Utc), "0", null, new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), null, new Guid("4facc425-b1ef-416a-979f-56da897448c5"), null, new Guid("b7a08d2e-116d-42e3-9ec5-1aa0636d116c"), new Guid("e58cabc9-e259-42ff-a2a1-0e8d39bb900e"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("9e1cf297-b90f-436a-8cd3-8ca95276872f"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2022, 11, 8, 8, 0, 0, 0, DateTimeKind.Utc), "1000000", null, new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), null, new Guid("4facc425-b1ef-416a-979f-56da897448c5"), null, new Guid("8445a2f4-97cd-45c9-921f-f649f85cc0be"), new Guid("e58cabc9-e259-42ff-a2a1-0e8d39bb900e"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("f32e24af-265d-4748-be59-769db539cb07"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2022, 11, 10, 10, 0, 0, 0, DateTimeKind.Utc), "3000000", null, new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), null, new Guid("4facc425-b1ef-416a-979f-56da897448c5"), null, new Guid("ccbb0db5-1661-4f9b-9482-67280ebdb6b5"), new Guid("e58cabc9-e259-42ff-a2a1-0e8d39bb900e"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                table: "QuickButtons",
                columns: new[] { "Id", "CreatedAt", "DoctorId", "Type", "UpdatedAt", "Value" },
                values: new object[,]
                {
                    { new Guid("074dacb8-0264-4a3b-8627-1581cf14ec3a"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), "Recommendations", new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Vitamin A" },
                    { new Guid("0b6747a5-9eea-4484-a192-6ad927025b84"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), "Interview", new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Muscle pain" },
                    { new Guid("0eeb68fc-ccdf-4102-b6da-2f9dd939bc3e"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), "Recommendations", new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Vitamin B12" },
                    { new Guid("179dfb23-44f4-49ec-bf10-159ef4ca6954"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), "Diagnosis", new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Chronic obstructive pulmonary disease" },
                    { new Guid("1deba9b9-0eeb-43d7-a95a-768a32a86f26"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), "Interview", new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Chest pain" },
                    { new Guid("1e310e93-070b-4202-8dc3-f514fce5a3b9"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), "Recommendations", new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Antihistamines" },
                    { new Guid("2027a099-57ce-4fbe-9d38-1c7c337a8e50"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), "Recommendations", new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Mucolytics" },
                    { new Guid("2c9c5afa-9331-4d23-87da-ac59217ac1fb"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), "Interview", new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Fatigue" },
                    { new Guid("2ea7fa99-42c7-4b75-a673-2435a2c50584"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), "Diagnosis", new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "COVID-19" },
                    { new Guid("30101274-f6d5-4b7e-9d06-b9e816c801b5"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), "Interview", new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Loss of taste" },
                    { new Guid("32368078-8d03-4ad2-8bc1-d683a94e82ed"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), "Diagnosis", new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Bronchitis" },
                    { new Guid("3652b57a-07e4-4915-b22a-581e76f58448"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), "Recommendations", new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Corticosteroids" },
                    { new Guid("41589b18-4df9-4f02-a101-af827964b3e3"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), "Recommendations", new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Antibiotics" },
                    { new Guid("47a50b1f-e874-4964-831b-d677941e9ecf"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), "Recommendations", new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Vitamin E" },
                    { new Guid("48ff8e51-0164-4194-90c5-9e0dd87b864d"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), "Diagnosis", new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Acute tonsillitis" },
                    { new Guid("4a87b3bd-5877-4388-9239-69f4e83bc322"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), "Recommendations", new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Antipyretics" },
                    { new Guid("521affee-54ee-48a5-ba36-0217e0bbb5d0"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), "Interview", new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Nausea" },
                    { new Guid("53e76e9d-6093-41f3-8de9-8d6c33434f71"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), "Recommendations", new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Fluids" },
                    { new Guid("57581436-4e1b-4856-bc53-d52c24c2fd65"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), "Recommendations", new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Bronchodilators" },
                    { new Guid("5aa836ee-00c1-4f7b-8c5e-b74d7162fe3c"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), "Recommendations", new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Antiviral drugs" },
                    { new Guid("5da4cdd1-9923-4cc9-aa08-eaf0e4003e0b"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), "Recommendations", new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Antidiarrheals" },
                    { new Guid("60898d97-32e0-4841-bec0-6ffd3f15cb9c"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), "Diagnosis", new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Acute bronchiti" },
                    { new Guid("650c86c0-c74e-4101-9777-4598873e1050"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), "Recommendations", new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Antiparasitic drugs" },
                    { new Guid("6f72df4d-d279-4fc4-bd22-67d3ba33cadc"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), "Diagnosis", new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Acute pharyngitis" },
                    { new Guid("7026dc95-3957-4779-bdc3-59294b8d8faa"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), "Interview", new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Shortness of breath" },
                    { new Guid("7582bbb7-9ecb-47d5-a742-891b514d60de"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), "Recommendations", new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Antacids" },
                    { new Guid("75c22761-f498-4973-86fb-cf1b13dd729e"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), "Diagnosis", new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Acute bronchitis" },
                    { new Guid("7a46084f-288c-4bbe-8da1-5ea97ca48ea6"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), "Recommendations", new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Expectorants" },
                    { new Guid("7d80a7b1-599d-457f-b8a3-7f963f0fed98"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), "Recommendations", new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Rest" },
                    { new Guid("7ff91ada-1ec7-46e4-b2d6-435e5cbbf38d"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), "Recommendations", new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Antitussives" },
                    { new Guid("814e13d2-0e02-4380-bc7f-692280ea68e2"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), "Interview", new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Sore throat" },
                    { new Guid("8a9ee513-9ff5-4b40-bb09-59730a829c77"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), "Diagnosis", new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Tuberculosis" },
                    { new Guid("8ea7023f-f79b-4dc7-965a-890cdd68ba9c"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), "Recommendations", new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Vitamin C" },
                    { new Guid("9390cf8d-f52a-4527-9f17-d785d281637e"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), "Recommendations", new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Decongestants" },
                    { new Guid("963dc7c0-fb0e-44b8-bf59-7048d241f807"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), "Interview", new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Confusion" },
                    { new Guid("9bb77950-3379-4c77-9213-b43950976939"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), "Recommendations", new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Antifungal drugs" },
                    { new Guid("9d7dd8b1-9cd9-4673-8900-6240a27d1847"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), "Interview", new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Vomiting" },
                    { new Guid("9dd0b024-e5dd-4169-bd91-391d6fbb615c"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), "Diagnosis", new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Influenza" },
                    { new Guid("a2c32b19-cd8f-495c-a11e-1c34eec818fe"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), "Recommendations", new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Vitamin D" },
                    { new Guid("a3c3d331-21c9-48eb-aace-ddffcd3d9331"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), "Recommendations", new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Antiemetics" },
                    { new Guid("a86c35ee-f3a0-4c18-8494-3e9c201dfcf0"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), "Diagnosis", new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Acute sinusitis" },
                    { new Guid("aca1f0bc-25f6-463f-aa93-45cb64cc5d7c"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), "Interview", new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Diarrhea" },
                    { new Guid("b0437003-e8c6-4cd5-858e-bc9cfe12357f"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), "Interview", new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Headache" },
                    { new Guid("c10ae801-7042-485f-a528-539503c568b7"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), "Recommendations", new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Antispasmodics" },
                    { new Guid("cc53e419-d5c6-4131-b77a-4040e279c83b"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), "Interview", new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Fever" },
                    { new Guid("cf407c2a-cf8a-404a-b07a-91f871e4a8cc"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), "Diagnosis", new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Pneumonia" },
                    { new Guid("d548b5e1-6f9b-44ec-bc33-0ed7f3f0e460"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), "Interview", new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Loss of smell" },
                    { new Guid("dcb02f21-0ed8-4c97-8f2f-2b58a42c6de3"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), "Recommendations", new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Vitamin K" },
                    { new Guid("df3f8ee7-a5e2-4c2a-a2f8-e598b560947d"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), "Interview", new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Cough" },
                    { new Guid("e015b806-ea24-4741-853c-2628d1393ad7"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), "Recommendations", new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Vitamin B6" },
                    { new Guid("e3a88a80-5c6d-4060-a8d7-7af07db8e3e4"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), "Recommendations", new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Analgesics" },
                    { new Guid("e53c4fab-e86a-4c03-9ad4-431846ff8467"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), "Diagnosis", new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Acute otitis media" },
                    { new Guid("e6d6e719-f349-417a-83e5-f9ef4c41543f"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), "Diagnosis", new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Common cold" }
                });

            migrationBuilder.InsertData(
                table: "Timetables",
                columns: new[] { "Id", "CreatedAt", "DoctorId", "EndDateTime", "StartDateTime", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("1b35a855-e2c1-4b52-abfb-5ff0a8835151"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), new DateTime(2022, 11, 7, 10, 0, 0, 0, DateTimeKind.Utc), new DateTime(2022, 11, 7, 7, 0, 0, 0, DateTimeKind.Utc), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("359aa6bc-73aa-424b-a759-59f7f5da6dcf"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), new DateTime(2022, 11, 8, 11, 0, 0, 0, DateTimeKind.Utc), new DateTime(2022, 11, 8, 8, 0, 0, 0, DateTimeKind.Utc), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("8967971c-193a-4c58-a326-9dced9182682"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), new DateTime(2022, 11, 9, 12, 0, 0, 0, DateTimeKind.Utc), new DateTime(2022, 11, 9, 9, 0, 0, 0, DateTimeKind.Utc), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("befcae26-e2c6-482f-b2bb-50309916b89e"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), new DateTime(2022, 11, 11, 14, 0, 0, 0, DateTimeKind.Utc), new DateTime(2022, 11, 11, 11, 0, 0, 0, DateTimeKind.Utc), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("fc2967c1-e0d8-4fa4-b7a0-f258ad53946e"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), new DateTime(2022, 11, 10, 13, 0, 0, 0, DateTimeKind.Utc), new DateTime(2022, 11, 10, 10, 0, 0, 0, DateTimeKind.Utc), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_DoctorId",
                table: "Appointments",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_PatientId",
                table: "Appointments",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_StatusId",
                table: "Appointments",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_TypeId",
                table: "Appointments",
                column: "TypeId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DrugItems_PrescriptionId",
                table: "DrugItems",
                column: "PrescriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_Prescriptions_AppointmentId",
                table: "Prescriptions",
                column: "AppointmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Prescriptions_DoctorId",
                table: "Prescriptions",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_Prescriptions_PatientId",
                table: "Prescriptions",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_QuickButtons_DoctorId",
                table: "QuickButtons",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_AppUserId",
                table: "RefreshTokens",
                column: "AppUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SickLeaves_AppointmentId",
                table: "SickLeaves",
                column: "AppointmentId");

            migrationBuilder.CreateIndex(
                name: "IX_SickLeaves_DoctorId",
                table: "SickLeaves",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_SickLeaves_PatientId",
                table: "SickLeaves",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_Timetables_DoctorId",
                table: "Timetables",
                column: "DoctorId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Admins");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "DrugItems");

            migrationBuilder.DropTable(
                name: "QuickButtons");

            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropTable(
                name: "SickLeaves");

            migrationBuilder.DropTable(
                name: "Timetables");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "Prescriptions");

            migrationBuilder.DropTable(
                name: "Appointments");

            migrationBuilder.DropTable(
                name: "AppointmentStatuses");

            migrationBuilder.DropTable(
                name: "AppointmentTypes");

            migrationBuilder.DropTable(
                name: "Doctors");

            migrationBuilder.DropTable(
                name: "Patients");

            migrationBuilder.DropTable(
                name: "AspNetUsers");
        }
    }
}
