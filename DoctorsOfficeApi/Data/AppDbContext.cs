using DoctorsOfficeApi.Entities;
using DoctorsOfficeApi.Entities.UserTypes;
using DoctorsOfficeApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DoctorsOfficeApi.Data;

public class AppDbContext : IdentityDbContext<AppUser, AppRole, Guid>
{
    public virtual DbSet<IdentityUserRole<Guid>> IdentityUserRole { get; set; } = default!;
    public virtual DbSet<Appointment> Appointments { get; set; } = default!;
    public virtual DbSet<AppointmentStatus> AppointmentStatuses { get; set; } = default!;
    public virtual DbSet<AppointmentType> AppointmentTypes { get; set; } = default!;
    public virtual DbSet<Doctor> Doctors { get; set; } = default!;
    public virtual DbSet<Patient> Patients { get; set; } = default!;
    public virtual DbSet<Admin> Admins { get; set; } = default!;

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<AppUser>().Ignore(user => user.LockoutEnabled);
        builder.Entity<AppUser>().Ignore(user => user.LockoutEnd);
        builder.Entity<IdentityUserToken<Guid>>().Metadata.SetIsTableExcludedFromMigrations(true);

        var adminRoleId = Guid.NewGuid();
        SeedRoles(builder, adminRoleId);
        CreateAdminAccount(builder, adminRoleId);
        SeedAppointmentTypes(builder);
        SeedAppointmentStatuses(builder);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        UpdateTimestamps();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    private static void SeedRoles(ModelBuilder builder, Guid adminRoleId)
    {
        var roles = new List<AppRole>
        {
            new() { Name = RoleTypes.Admin, NormalizedName = RoleTypes.Admin.ToUpper(), Id = adminRoleId },
            new() { Name = RoleTypes.Patient, NormalizedName = RoleTypes.Patient.ToUpper(), Id = Guid.NewGuid() },
            new() { Name = RoleTypes.Doctor, NormalizedName = RoleTypes.Doctor.ToUpper(), Id = Guid.NewGuid() }
        };
        builder.Entity<AppRole>().HasData(roles);
    }

    private static void CreateAdminAccount(ModelBuilder builder, Guid adminRoleId)
    {
        var adminUserId = Guid.NewGuid();
        var hasher = new PasswordHasher<AppUser>();
        builder.Entity<AppUser>().HasData(
            new AppUser
            {
                Id = adminUserId,
                UserName = "admin",
                NormalizedUserName = "admin".ToUpper(),
                PasswordHash = hasher.HashPassword(null!, "admin"),
                SecurityStamp = Guid.NewGuid().ToString()
            }
        );
        builder.Entity<IdentityUserRole<Guid>>().HasData(
            new IdentityUserRole<Guid>
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

    private void UpdateTimestamps()
    {
        var entityEntries = ChangeTracker.Entries()
            .Where(x => x.Entity is BaseEntity && x.State is EntityState.Added or EntityState.Modified);

        foreach (var entityEntry in entityEntries)
        {
            var now = DateTime.UtcNow;
            ((BaseEntity)entityEntry.Entity).UpdatedAt = now;
            if (entityEntry.State == EntityState.Added)
                ((BaseEntity)entityEntry.Entity).CreatedAt = now;
        }
    }

    private static void SeedAppointmentTypes(ModelBuilder builder)
    {
        var appointmentTypeFields = typeof(AppointmentTypes).GetFields();
        var appointmentTypes = appointmentTypeFields
            .Select(appointmentTypeField => new AppointmentType { Id = Guid.NewGuid(), Name = appointmentTypeField.Name })
            .ToList();
        builder.Entity<AppointmentType>().HasData(appointmentTypes);
    }

    private static void SeedAppointmentStatuses(ModelBuilder builder)
    {
        var appointmentStatusFields = typeof(AppointmentStatuses).GetFields().Where(field => field.Name != "AllowedTransitions");
        var appointmentStatuses = appointmentStatusFields
            .Select(appointmentStatusField => new AppointmentStatus { Id = Guid.NewGuid(), Name = appointmentStatusField.Name })
            .ToList();
        builder.Entity<AppointmentStatus>().HasData(appointmentStatuses);
    }
}