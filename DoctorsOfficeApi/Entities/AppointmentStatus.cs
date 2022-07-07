using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoctorsOfficeApi.Entities;

[Table("AppointmentStatuses")]
public class AppointmentStatus
{
    [Key] public long Id { get; set; }
    public string Name { get; set; } = default!;
}