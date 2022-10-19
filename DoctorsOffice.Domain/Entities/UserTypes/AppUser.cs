using Microsoft.AspNetCore.Identity;

namespace DoctorsOffice.Domain.Entities.UserTypes;

public class AppUser : IdentityUser<Guid>
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public virtual List<RefreshToken> RefreshTokens { get; set; } = new();
}