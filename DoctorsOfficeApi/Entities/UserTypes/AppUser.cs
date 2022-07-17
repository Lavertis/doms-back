using Microsoft.AspNetCore.Identity;

namespace DoctorsOfficeApi.Entities.UserTypes;

public class AppUser : IdentityUser<Guid>
{
    public virtual List<RefreshToken> RefreshTokens { get; set; } = new();
}