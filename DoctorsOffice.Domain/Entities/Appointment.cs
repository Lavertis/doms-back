using System.ComponentModel.DataAnnotations.Schema;
using DoctorsOffice.Domain.Entities.UserTypes;

namespace DoctorsOffice.Domain.Entities;

[Table("Appointments")]
public class Appointment : BaseEntity
{
    public DateTime Date { get; set; }
    public string Description { get; set; } = null!;

    public string? Interview { get; set; }
    public string? Diagnosis { get; set; }
    public string? Recommendations { get; set; }

    public virtual Patient Patient { get; set; } = default!;
    public virtual Guid PatientId { get; set; }

    public virtual Doctor Doctor { get; set; } = default!;
    public virtual Guid DoctorId { get; set; }

    public virtual AppointmentStatus Status { get; set; } = default!;
    public virtual Guid StatusId { get; set; }

    public virtual AppointmentType Type { get; set; } = default!;
    public virtual Guid TypeId { get; set; }

    public List<Prescription> Prescriptions { get; set; } = new();
}