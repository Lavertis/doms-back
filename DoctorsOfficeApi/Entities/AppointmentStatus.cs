using System.ComponentModel.DataAnnotations.Schema;

namespace DoctorsOfficeApi.Entities;

[Table("AppointmentStatuses")]
public class AppointmentStatus : BaseEntity
{
    public string Name { get; set; } = default!;
}