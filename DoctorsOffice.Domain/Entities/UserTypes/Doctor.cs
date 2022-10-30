using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoctorsOffice.Domain.Entities.UserTypes;

[Table("Doctors")]
public class Doctor : BaseEntity
{
    [Key] [ForeignKey(nameof(AppUser))] public new Guid Id { get; set; }
    public AppUser AppUser { get; set; } = default!;
}