using System.ComponentModel.DataAnnotations.Schema;

namespace DoctorsOfficeApi.Entities;

[Table("AppointmentTypes")]
public class AppointmentType : BaseEntity
{
    public string Name { get; set; } = default!;
}