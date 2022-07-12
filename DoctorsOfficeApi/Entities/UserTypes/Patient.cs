using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoctorsOfficeApi.Entities.UserTypes;

public class Patient
{
    [Key] [ForeignKey(nameof(AppUser))] public string Id { get; set; } = default!;
    public virtual AppUser AppUser { get; set; } = default!;
    public string UserName => AppUser.UserName;
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string Email => AppUser.Email;
    public string PhoneNumber => AppUser.PhoneNumber;
    public string Address { get; set; } = default!;
    public DateTime DateOfBirth { get; set; } = default!;
}