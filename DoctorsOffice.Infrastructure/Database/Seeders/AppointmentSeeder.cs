using DoctorsOffice.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DoctorsOffice.Infrastructure.Database.Seeders;

public static class AppointmentSeeder
{
    private static readonly string[] AppointmentIds =
    {
        "56a26dea-caf2-4a4b-a013-ed9e776d25dc",
        "9e1cf297-b90f-436a-8cd3-8ca95276872f",
        "4f319bc2-a6d9-4a52-9357-0772d0edd639",
        "f32e24af-265d-4748-be59-769db539cb07",
        "2cf674a8-9311-4515-a6bb-8d8094ade09c"
    };

    public static void SeedAppointments(ModelBuilder builder)
    {
        var appointments = new List<Appointment>();
        for (var i = 0; i < AppointmentIds.Length; i++)
        {
            var appointment = new Appointment
            {
                Id = Guid.Parse(AppointmentIds[i]),
                Date = new DateTime(2022, 11, i + 1, 0, 0, 0, DateTimeKind.Utc),
                Description = (i * 1_000_000).ToString(),
                DoctorId = Guid.Parse(UserSeeder.DoctorUserId),
                PatientId = Guid.Parse(UserSeeder.PatientUserId),
                StatusId = Guid.Parse(AppointmentStatusSeeder.PendingStatusId),
                TypeId = Guid.Parse(AppointmentTypeSeeder.CheckupTypeId),
                CreatedAt = DatabaseSeeder.TimeStamp,
                UpdatedAt = DatabaseSeeder.TimeStamp
            };
            appointments.Add(appointment);
        }

        builder.Entity<Appointment>().HasData(appointments);
    }
}