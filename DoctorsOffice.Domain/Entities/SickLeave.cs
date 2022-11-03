using System.ComponentModel.DataAnnotations.Schema;
using DoctorsOffice.Domain.Entities.UserTypes;

namespace DoctorsOffice.Domain.Entities;

[Table(("SickLeaves"))]
public class SickLeave : BaseEntity
{
    public virtual Patient Patient { get; set; } = default!;

    public virtual Guid PatientId { get; set; }
    
    public virtual Doctor Doctor { get; set; } = default!;
    
    public virtual Appointment? Appointment { get; set; }

    public virtual Guid? AppointmentId { get; set; }
    public virtual Guid DoctorId { get; set; }
    
    public DateTime DateStart { get; set; }
    
    public DateTime DateEnd { get; set; }

    public string Diagnosis { get; set; } = "";

    public string Purpose { get; set; } = "";
}