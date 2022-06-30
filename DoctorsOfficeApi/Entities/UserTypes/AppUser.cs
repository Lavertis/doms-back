using Microsoft.AspNetCore.Identity;

namespace DoctorsOfficeApi.Entities.UserTypes;

public class AppUser : IdentityUser
{
    public virtual List<RefreshToken> RefreshTokens { get; set; } = new();
}