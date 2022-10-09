using DoctorsOffice.Domain.Entities.UserTypes;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DoctorsOffice.Infrastructure.Database.Seeders;

public static class UserSeeder
{
    private const string AdminUserId = "f2f0ccba-ce3c-4ce4-8167-b79d88117c05";

    public static void SeedUsers(ModelBuilder builder)
    {
        SeedAdminAccount(builder);
    }

    private static void SeedAdminAccount(ModelBuilder builder)
    {
        var adminAppUser = new AppUser
        {
            Id = Guid.Parse(AdminUserId),
            UserName = "admin",
            NormalizedUserName = "admin".ToUpper(),
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
}