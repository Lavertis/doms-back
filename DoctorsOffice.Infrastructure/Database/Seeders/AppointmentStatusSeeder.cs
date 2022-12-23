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
                Id = AppointmentStatuses.Accepted.Id,
                Name = AppointmentStatuses.Accepted.Name
            },
            new()
            {
                Id = AppointmentStatuses.Cancelled.Id,
                Name = AppointmentStatuses.Cancelled.Name
            },
            new()
            {
                Id = AppointmentStatuses.Completed.Id,
                Name = AppointmentStatuses.Completed.Name
            },
            new()
            {
                Id = AppointmentStatuses.Pending.Id,
                Name = AppointmentStatuses.Pending.Name
            },
            new()
            {
                Id = AppointmentStatuses.Rejected.Id,
                Name = AppointmentStatuses.Rejected.Name
            }
        };

        foreach (var appointmentStatus in appointmentStatuses)
        {
            appointmentStatus.CreatedAt = appointmentStatus.UpdatedAt = DatabaseSeeder.TimeStamp;
        }

        builder.Entity<AppointmentStatus>().HasData(appointmentStatuses);
    }
}