using DoctorsOffice.Domain.Entities;
using DoctorsOffice.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace DoctorsOffice.Infrastructure.Database.Seeders;

public static class AppointmentTypeSeeder
{
    public const string CheckupTypeId = "e58cabc9-e259-42ff-a2a1-0e8d39bb900e";
    public const string ConsultationTypeId = "532ec4d6-a4ad-4ece-a0b5-9f03e1033bf5";

    public static void SeedAppointmentTypes(ModelBuilder builder)
    {
        var appointmentTypes = new List<AppointmentType>
        {
            new()
            {
                Id = Guid.Parse(CheckupTypeId),
                Name = AppointmentTypes.Checkup
            },
            new()
            {
                Id = Guid.Parse(ConsultationTypeId),
                Name = AppointmentTypes.Consultation
            }
        };

        foreach (var appointmentType in appointmentTypes)
        {
            appointmentType.CreatedAt = appointmentType.UpdatedAt = DatabaseSeeder.TimeStamp;
        }

        builder.Entity<AppointmentType>().HasData(appointmentTypes);
    }
}