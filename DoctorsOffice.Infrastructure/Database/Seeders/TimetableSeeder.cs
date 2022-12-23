using DoctorsOffice.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DoctorsOffice.Infrastructure.Database.Seeders;

public class TimetableSeeder
{
    private static readonly string[] TimetableIds =
    {
        "1b35a855-e2c1-4b52-abfb-5ff0a8835151",
        "359aa6bc-73aa-424b-a759-59f7f5da6dcf",
        "8967971c-193a-4c58-a326-9dced9182682",
        "fc2967c1-e0d8-4fa4-b7a0-f258ad53946e",
        "befcae26-e2c6-482f-b2bb-50309916b89e"
    };

    public static void SeedTimetables(ModelBuilder builder)
    {
        var timetables = new List<Timetable>();
        for (var i = 0; i < TimetableIds.Length; i++)
        {
            var timetable = new Timetable
            {
                Id = Guid.Parse(TimetableIds[i]),
                StartDateTime = new DateTime(2022, 11, i + 7, 7 + i, 0, 0, DateTimeKind.Utc),
                EndDateTime = new DateTime(2022, 11, i + 7, 10 + i, 0, 0, DateTimeKind.Utc),
                DoctorId = Guid.Parse(UserSeeder.DoctorUserId),
                CreatedAt = DatabaseSeeder.TimeStamp,
                UpdatedAt = DatabaseSeeder.TimeStamp
            };
            timetables.Add(timetable);
        }

        builder.Entity<Timetable>().HasData(timetables);
    }
}