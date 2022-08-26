using System.ComponentModel.DataAnnotations.Schema;

namespace DoctorsOffice.Domain.Entities;

[Table("AppointmentTypes")]
public class AppointmentType : BaseEntity
{
    public string Name { get; set; } = null!;
}