using System.ComponentModel.DataAnnotations.Schema;
using DoctorsOffice.Domain.Entities.UserTypes;

namespace DoctorsOffice.Domain.Entities;

[Table("Prescriptions")]
public class Prescription : BaseEntity
{
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public Doctor Doctor { get; set; } = default!;
    public Guid DoctorId { get; set; }
    public Patient Patient { get; set; } = default!;
    public Guid PatientId { get; set; }
    public IList<DrugItem> DrugItems { get; set; } = default!;
}