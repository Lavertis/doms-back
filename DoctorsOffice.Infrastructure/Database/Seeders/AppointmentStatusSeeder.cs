using DoctorsOffice.Domain.Entities;
using DoctorsOffice.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace DoctorsOffice.Infrastructure.Database.Seeders;

public static class AppointmentStatusSeeder
{
    public static void SeedAppointmentStatuses(ModelBuilder builder)
    {
        var appointmentStatuses = new List<AppointmentStatus>
        {
            new()
            {
                Id = Guid.Parse("8445a2f4-97cd-45c9-921f-f649f85cc0be"),
                Name = AppointmentStatuses.Accepted
            },
            new()
            {
                Id = Guid.Parse("ccbb0db5-1661-4f9b-9482-67280ebdb6b5"),
                Name = AppointmentStatuses.Cancelled
            },
            new()
            {
                Id = Guid.Parse("5de8a7ba-fb65-464f-9583-181d20d44b1b"),
                Name = AppointmentStatuses.Completed
            },
            new()
            {
                Id = Guid.Parse("b7a08d2e-116d-42e3-9ec5-1aa0636d116c"),
                Name = AppointmentStatuses.Pending
            },
            new()
            {
                Id = Guid.Parse("1cf993e4-73f2-497f-ad38-bccb4b4d0eee"),
                Name = AppointmentStatuses.Rejected
            }
        };

        foreach (var appointmentStatus in appointmentStatuses)
        {
            appointmentStatus.CreatedAt = appointmentStatus.UpdatedAt = DatabaseSeeder.TimeStamp;
        }

        builder.Entity<AppointmentStatus>().HasData(appointmentStatuses);
    }
}