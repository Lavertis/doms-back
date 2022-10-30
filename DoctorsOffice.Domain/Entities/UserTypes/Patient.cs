using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoctorsOffice.Domain.Entities.UserTypes;

[Table("Patients")]
public class Patient : BaseEntity
{
    [Key] [ForeignKey(nameof(AppUser))] public new Guid Id { get; set; }
    public AppUser AppUser { get; set; } = null!;
    public string NationalId { get; set; } = null!;
    public string Address { get; set; } = null!;
    public DateTime DateOfBirth { get; set; }
}