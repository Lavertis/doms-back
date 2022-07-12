using DoctorsOfficeApi.Entities;
using DoctorsOfficeApi.Entities.UserTypes;
using DoctorsOfficeApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DoctorsOfficeApi.Data;

public class AppDbContext : IdentityDbContext<AppUser>
{
    public virtual DbSet<IdentityUserRole<string>> IdentityUserRole { get; set; } = default!;
    public virtual DbSet<Appointment> Appointments { get; set; } = default!;
    public virtual DbSet<AppointmentStatus> AppointmentStatuses { get; set; } = default!;
    public virtual DbSet<AppointmentType> AppointmentTypes { get; set; } = default!;
    public virtual DbSet<Doctor> Doctors { get; set; } = default!;
    public virtual DbSet<Patient> Patients { get; set; } = default!;
    public virtual DbSet<Admin> Admins { get; set; } = default!;

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.UseLazyLoadingProxies();
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<AppUser>().Ignore(user => user.LockoutEnabled);
        builder.Entity<AppUser>().Ignore(user => user.LockoutEnd);
        builder.Entity<IdentityUserToken<string>>().Metadata.SetIsTableExcludedFromMigrations(true);

        const string adminRoleId = "fa2640a0-0496-4010-bc27-424e0e5c6f78";
        SeedRoles(builder, adminRoleId);
        CreateAdminAccount(builder, adminRoleId);
        SeedAppointmentTypes(builder);
        SeedAppointmentStatuses(builder);
    }

    private static void SeedRoles(ModelBuilder builder, string adminRoleId)
    {
        var roles = new List<IdentityRole>
        {
            new() { Name = RoleTypes.Admin, NormalizedName = RoleTypes.Admin.ToUpper(), Id = adminRoleId },
            new() { Name = RoleTypes.Patient, NormalizedName = RoleTypes.Patient.ToUpper() },
            new() { Name = RoleTypes.Doctor, NormalizedName = RoleTypes.Doctor.ToUpper() }
        };
        builder.Entity<IdentityRole>().HasData(roles);
    }

    private static void CreateAdminAccount(ModelBuilder builder, string adminRoleId)
    {
        const string adminUserId = "7a4165b4-0aca-43fb-a390-294781ee377f";
        var hasher = new PasswordHasher<AppUser>();
        builder.Entity<AppUser>().HasData(
            new AppUser
            {
                Id = adminUserId,
                UserName = "admin",
                NormalizedUserName = "admin".ToUpper(),
                PasswordHash = hasher.HashPassword(null!, "admin")
            }
        );
        builder.Entity<IdentityUserRole<string>>().HasData(
            new IdentityUserRole<string>
            {
                RoleId = adminRoleId,
                UserId = adminUserId
            }
        );
        builder.Entity<Admin>().HasData(
            new Admin
            {
                Id = adminUserId
            }
        );
    }

    private static void SeedAppointmentTypes(ModelBuilder builder)
    {
        var appointmentTypeFields = typeof(AppointmentTypes).GetFields();
        var id = 1;
        var appointmentTypes = appointmentTypeFields
            .Select(appointmentTypeField => new AppointmentType { Id = id++, Name = appointmentTypeField.Name })
            .ToList();
        builder.Entity<AppointmentType>().HasData(appointmentTypes);
    }

    private static void SeedAppointmentStatuses(ModelBuilder builder)
    {
        var appointmentStatusFields = typeof(AppointmentStatuses).GetFields().Where(field => field.Name != "AllowedTransitions");
        var id = 1;
        var appointmentStatuses = appointmentStatusFields
            .Select(appointmentStatusField => new AppointmentStatus { Id = id++, Name = appointmentStatusField.Name })
            .ToList();
        builder.Entity<AppointmentStatus>().HasData(appointmentStatuses);
    }
}