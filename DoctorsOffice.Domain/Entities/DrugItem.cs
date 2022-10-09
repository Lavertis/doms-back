using System.ComponentModel.DataAnnotations.Schema;

namespace DoctorsOffice.Domain.Entities;

[Table("DrugItems")]
public class DrugItem : BaseEntity
{
    public long Rxcui { get; set; }
    public string Name { get; set; } = null!;
    public int Quantity { get; set; }
    public string Dosage { get; set; } = null!;

    public Prescription Prescription { get; set; } = null!;
    public Guid PrescriptionId { get; set; }
}