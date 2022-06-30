using DoctorsOfficeApi.Entities.UserTypes;
using DoctorsOfficeApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DoctorsOfficeApi.Data;

public class AppDbContext : IdentityDbContext<AppUser>
{
    public DbSet<IdentityUserRole<string>> IdentityUserRole { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<AppUser>().Ignore(user => user.LockoutEnabled);
        builder.Entity<AppUser>().Ignore(user => user.LockoutEnd);
        builder.Entity<AppUser>().Ignore(user => user.PhoneNumber);
        builder.Entity<IdentityUserToken<string>>().Metadata.SetIsTableExcludedFromMigrations(true);

        const string adminRoleId = "fa2640a0-0496-4010-bc27-424e0e5c6f78";
        SeedRoles(builder, adminRoleId);
        CreateAdminAccount(builder, adminRoleId);
    }

    private void SeedRoles(ModelBuilder builder, string adminRoleId)
    {
        var roles = new List<IdentityRole>
        {
            new() { Name = RoleType.Admin, NormalizedName = RoleType.Admin.ToUpper(), Id = adminRoleId },
            new() { Name = RoleType.Patient, NormalizedName = RoleType.Patient.ToUpper() },
            new() { Name = RoleType.Doctor, NormalizedName = RoleType.Doctor.ToUpper() }
        };
        builder.Entity<IdentityRole>().HasData(roles);
    }

    private void CreateAdminAccount(ModelBuilder builder, string adminRoleId)
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
}