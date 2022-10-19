using DoctorsOffice.Domain.Entities.UserTypes;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DoctorsOffice.Infrastructure.Database.Seeders;

public static class UserSeeder
{
    private const string AdminUserId = "f2f0ccba-ce3c-4ce4-8167-b79d88117c05";
    public const string DoctorUserId = "c8934fff-2f5a-4198-893f-26023d8f4107";
    public const string PatientUserId = "4facc425-b1ef-416a-979f-56da897448c5";

    public static void SeedUsers(ModelBuilder builder)
    {
        SeedAdminAccount(builder);
        SeedDoctor(builder);
        SeedPatient(builder);
    }

    private static void SeedAdminAccount(ModelBuilder builder)
    {
        var adminAppUser = new AppUser
        {
            Id = Guid.Parse(AdminUserId),
            UserName = "admin",
            NormalizedUserName = "ADMIN",
            FirstName = "Admin",
            LastName = "Admin",
            PasswordHash = "ACwoXDy/z+O6bjrLgviDbsZ036YrMsYj/fMPviVIsW1welLPf0g9dCgRkUTW3JOSpA==", // admin
            SecurityStamp = AdminUserId,
            ConcurrencyStamp = AdminUserId
        };
        var adminUserRole = new IdentityUserRole<Guid>
        {
            RoleId = Guid.Parse(RoleSeeder.AdminRoleId),
            UserId = Guid.Parse(AdminUserId)
        };
        var admin = new Admin
        {
            Id = Guid.Parse(AdminUserId),
            CreatedAt = DatabaseSeeder.TimeStamp,
            UpdatedAt = DatabaseSeeder.TimeStamp
        };
        builder.Entity<AppUser>().HasData(adminAppUser);
        builder.Entity<IdentityUserRole<Guid>>().HasData(adminUserRole);
        builder.Entity<Admin>().HasData(admin);
    }

    private static void SeedDoctor(ModelBuilder builder)
    {
        var doctorAppUser = new AppUser
        {
            Id = Guid.Parse(DoctorUserId),
            UserName = "doctor",
            NormalizedUserName = "DOCTOR",
            Email = "doctor@doctor.com",
            NormalizedEmail = "DOCTOR@DOCTOR.COM",
            PhoneNumber = "123456789",
            FirstName = "Doctor",
            LastName = "Doctor",
            PasswordHash = "AMbTv46BLUYaRTuuF5U53eDGMBRw4T7wQwaxSxTrM4mPB87g87fP+FW4n+ecgCXCdg==", // doctor
            SecurityStamp = DoctorUserId,
            ConcurrencyStamp = DoctorUserId
        };
        var doctorUserRole = new IdentityUserRole<Guid>
        {
            RoleId = Guid.Parse(RoleSeeder.DoctorRoleId),
            UserId = Guid.Parse(DoctorUserId)
        };
        var doctor = new Doctor
        {
            Id = Guid.Parse(DoctorUserId),
            CreatedAt = DatabaseSeeder.TimeStamp,
            UpdatedAt = DatabaseSeeder.TimeStamp
        };
        builder.Entity<AppUser>().HasData(doctorAppUser);
        builder.Entity<IdentityUserRole<Guid>>().HasData(doctorUserRole);
        builder.Entity<Doctor>().HasData(doctor);
    }

    private static void SeedPatient(ModelBuilder builder)
    {
        var patientAppUser = new AppUser
        {
            Id = Guid.Parse(PatientUserId),
            UserName = "patient",
            NormalizedUserName = "PATIENT",
            Email = "patient@patient.com",
            NormalizedEmail = "PATIENT@PATIENT.COM",
            PhoneNumber = "123456789",
            FirstName = "Patient",
            LastName = "Patient",
            PasswordHash = "AL9EaDGX0cdo1q6ldEn3SDtSYoYHcRpcEBXmM4TUfF+hOIT06L6ZfvndiURMFQEphw==", // patient
            SecurityStamp = PatientUserId,
            ConcurrencyStamp = PatientUserId
        };
        var patientUserRole = new IdentityUserRole<Guid>
        {
            RoleId = Guid.Parse(RoleSeeder.PatientRoleId),
            UserId = Guid.Parse(PatientUserId)
        };
        var patient = new Patient
        {
            Id = Guid.Parse(PatientUserId),
            NationalId = "04233040549",
            Address = "7865 Greenview St. Randallstown, MD 21133",
            DateOfBirth = new DateTime(2000, 08, 10, 0, 0, 0, DateTimeKind.Utc),
            CreatedAt = DatabaseSeeder.TimeStamp,
            UpdatedAt = DatabaseSeeder.TimeStamp
        };
        builder.Entity<AppUser>().HasData(patientAppUser);
        builder.Entity<IdentityUserRole<Guid>>().HasData(patientUserRole);
        builder.Entity<Patient>().HasData(patient);
    }
}