using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoctorsOfficeApi.Entities.UserTypes;

[Table("Patients")]
public class Patient
{
    [Key] [ForeignKey(nameof(AppUser))] public Guid Id { get; set; }
    public virtual AppUser AppUser { get; set; } = default!;
    public string UserName => AppUser.UserName;
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string Email => AppUser.Email;
    public string PhoneNumber => AppUser.PhoneNumber;
    public string Address { get; set; } = default!;
    public DateTime DateOfBirth { get; set; } = default!;
}