using System.ComponentModel.DataAnnotations.Schema;
using DoctorsOffice.Domain.Entities.UserTypes;

namespace DoctorsOffice.Domain.Entities;

[Table("Prescriptions")]
public class Prescription : BaseEntity
{
    public DateTime FulfillmentDeadline { get; set; }

    public Doctor Doctor { get; set; } = default!;
    public Guid DoctorId { get; set; }

    public Patient Patient { get; set; } = default!;
    public Guid PatientId { get; set; }

    public Appointment? Appointment { get; set; }
    public Guid? AppointmentId { get; set; }

    public List<DrugItem> DrugItems { get; set; } = new();
}