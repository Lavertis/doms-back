using Microsoft.EntityFrameworkCore;

namespace DoctorsOffice.Infrastructure.Database.Seeders;

public static class DatabaseSeeder
{
    public static readonly DateTime TimeStamp = new(2022, 08, 10, 0, 0, 0, DateTimeKind.Utc);

    public static void SeedDatabase(ModelBuilder builder)
    {
        RoleSeeder.SeedRoles(builder);
        UserSeeder.SeedUsers(builder);
        AppointmentTypeSeeder.SeedAppointmentTypes(builder);
        AppointmentStatusSeeder.SeedAppointmentStatuses(builder);
        AppointmentSeeder.SeedAppointments(builder);
    }
}