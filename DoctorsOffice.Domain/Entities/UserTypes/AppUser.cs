using Microsoft.AspNetCore.Identity;

namespace DoctorsOffice.Domain.Entities.UserTypes;

public class AppUser : IdentityUser<Guid>
{
    public virtual List<RefreshToken> RefreshTokens { get; set; } = new();
}