using DoctorsOffice.Domain.Entities;
using DoctorsOffice.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace DoctorsOffice.Infrastructure.Database.Seeders;

public static class RoleSeeder
{
    public const string AdminRoleId = "6506ab69-c793-4d0a-87d4-6565e98523d4";
    private const string PatientRoleId = "d4349d0c-d18c-4324-be02-254ad1208004";
    private const string DoctorRoleId = "80389a16-fbd0-4db1-b655-05a29d202a75";

    public static void SeedRoles(ModelBuilder builder)
    {
        var roles = new List<AppRole>
        {
            new()
            {
                Id = Guid.Parse(AdminRoleId),
                Name = Roles.Admin,
                NormalizedName = Roles.Admin.ToUpper(),
                ConcurrencyStamp = AdminRoleId
            },
            new()
            {
                Id = Guid.Parse(PatientRoleId),
                Name = Roles.Patient,
                NormalizedName = Roles.Patient.ToUpper(),
                ConcurrencyStamp = PatientRoleId
            },
            new()
            {
                Id = Guid.Parse(DoctorRoleId),
                Name = Roles.Doctor,
                NormalizedName = Roles.Doctor.ToUpper(),
                ConcurrencyStamp = DoctorRoleId
            }
        };
        builder.Entity<AppRole>().HasData(roles);
    }
}