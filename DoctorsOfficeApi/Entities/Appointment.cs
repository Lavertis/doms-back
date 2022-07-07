using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DoctorsOfficeApi.Entities.UserTypes;

namespace DoctorsOfficeApi.Entities;

[Table("Appointments")]
public class Appointment
{
    [Key] public long Id { get; set; }
    public DateTime Date { get; set; }
    public string Description { get; set; } = default!;
    public virtual Patient Patient { get; set; } = default!;
    public virtual Doctor Doctor { get; set; } = default!;
    public virtual AppointmentStatus Status { get; set; } = default!;
    public virtual AppointmentType Type { get; set; } = default!;
}