using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoctorsOfficeApi.Entities.UserTypes;

public class Doctor
{
    [Key] [ForeignKey(nameof(AppUser))] public string Id { get; set; } = default!;
    public virtual AppUser AppUser { get; set; } = default!;

    public string UserName => AppUser.UserName;
    public string Email => AppUser.Email;
    public string PhoneNumber => AppUser.PhoneNumber;
}