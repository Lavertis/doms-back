using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoctorsOfficeApi.Entities;

[Table("AppointmentTypes")]
public class AppointmentType
{
    [Key] public long Id { get; set; }
    public string Name { get; set; } = default!;
}