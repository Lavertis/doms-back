using System.ComponentModel.DataAnnotations.Schema;
using DoctorsOffice.Domain.Entities.UserTypes;

namespace DoctorsOffice.Domain.Entities;

[Table(("SickLeaves"))]
public class SickLeave : BaseEntity
{
    public Patient Patient { get; set; } = default!;

    public Guid PatientId { get; set; }

    public Doctor Doctor { get; set; } = default!;

    public Appointment? Appointment { get; set; }

    public Guid? AppointmentId { get; set; }
    public Guid DoctorId { get; set; }

    public DateTime DateStart { get; set; }

    public DateTime DateEnd { get; set; }

    public string Diagnosis { get; set; } = "";

    public string Purpose { get; set; } = "";
}