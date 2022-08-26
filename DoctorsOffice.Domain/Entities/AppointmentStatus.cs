using System.ComponentModel.DataAnnotations.Schema;

namespace DoctorsOffice.Domain.Entities;

[Table("AppointmentStatuses")]
public class AppointmentStatus : BaseEntity
{
    public string Name { get; set; } = null!;
}