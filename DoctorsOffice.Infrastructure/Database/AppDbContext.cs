using DoctorsOffice.Domain.Entities;
using DoctorsOffice.Domain.Entities.UserTypes;
using DoctorsOffice.Infrastructure.Database.Seeders;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DoctorsOffice.Infrastructure.Database;

public class AppDbContext : IdentityDbContext<AppUser, AppRole, Guid>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public virtual DbSet<IdentityUserRole<Guid>> IdentityUserRole { get; set; } = default!;
    public virtual DbSet<Appointment> Appointments { get; set; } = default!;
    public virtual DbSet<AppointmentStatus> AppointmentStatuses { get; set; } = default!;
    public virtual DbSet<AppointmentType> AppointmentTypes { get; set; } = default!;
    public virtual DbSet<Doctor> Doctors { get; set; } = default!;
    public virtual DbSet<Patient> Patients { get; set; } = default!;
    public virtual DbSet<Admin> Admins { get; set; } = default!;
    public virtual DbSet<Prescription> Prescriptions { get; set; } = default!;
    public virtual DbSet<DrugItem> DrugItems { get; set; } = default!;

    public virtual DbSet<SickLeave> SickLeaves { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<AppUser>().Ignore(user => user.LockoutEnabled);
        builder.Entity<AppUser>().Ignore(user => user.LockoutEnd);
        builder.Entity<IdentityUserToken<Guid>>().Metadata.SetIsTableExcludedFromMigrations(true);
        DatabaseSeeder.SeedDatabase(builder);
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

    private void UpdateTimestamps()
    {
        var entityEntries = ChangeTracker.Entries()
            .Where(x => x.Entity is BaseEntity && x.State is EntityState.Added or EntityState.Modified);

        foreach (var entityEntry in entityEntries)
        {
            var now = DateTime.UtcNow;
            ((BaseEntity) entityEntry.Entity).UpdatedAt = now;
            if (entityEntry.State == EntityState.Added)
                ((BaseEntity) entityEntry.Entity).CreatedAt = now;
        }
    }
}