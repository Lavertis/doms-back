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

    public Patient Patient { get; set; } = default!;
    public Guid PatientId { get; set; }

    public Doctor Doctor { get; set; } = default!;
    public Guid DoctorId { get; set; }

    public AppointmentStatus Status { get; set; } = default!;
    public Guid StatusId { get; set; }

    public AppointmentType Type { get; set; } = default!;
    public Guid TypeId { get; set; }

    public List<Prescription> Prescriptions { get; set; } = new();

    public List<SickLeave> SickLeaves { get; set; } = new();
}