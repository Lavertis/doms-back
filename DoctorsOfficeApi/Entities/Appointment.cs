using System.ComponentModel.DataAnnotations.Schema;
using DoctorsOfficeApi.Entities.UserTypes;

namespace DoctorsOfficeApi.Entities;

[Table("Appointments")]
public class Appointment : BaseEntity
{
    public DateTime Date { get; set; }
    public string Description { get; set; } = default!;
    public virtual Patient Patient { get; set; } = default!;
    public virtual Guid PatientId { get; set; }
    public virtual Doctor Doctor { get; set; } = default!;
    public virtual Guid DoctorId { get; set; }
    public virtual AppointmentStatus Status { get; set; } = default!;
    public virtual Guid StatusId { get; set; }
    public virtual AppointmentType Type { get; set; } = default!;
    public virtual Guid TypeId { get; set; }
}