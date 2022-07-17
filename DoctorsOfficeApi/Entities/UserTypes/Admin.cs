using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoctorsOfficeApi.Entities.UserTypes;

[Table("Admins")]
public class Admin
{
    [Key] [ForeignKey(nameof(AppUser))] public Guid Id { get; set; }
    public virtual AppUser AppUser { get; set; } = default!;
}