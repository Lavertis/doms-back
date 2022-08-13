using DoctorsOffice.Domain.Entities.UserTypes;

namespace DoctorsOfficeApi.Models.Responses;

public class AdminResponse
{
    public AdminResponse()
    {
    }

    public AdminResponse(Admin admin)
    {
        Id = admin.Id;
        UserName = admin.AppUser.UserName;
        Email = admin.AppUser.Email;
    }

    public AdminResponse(AppUser appUser)
    {
        Id = appUser.Id;
        UserName = appUser.UserName;
        Email = appUser.Email;
    }

    public Guid Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = default!;
}