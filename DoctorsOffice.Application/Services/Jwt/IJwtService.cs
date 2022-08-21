using System.Security.Claims;
using DoctorsOffice.Domain.Entities.UserTypes;

namespace DoctorsOffice.Application.Services.Jwt;

public interface IJwtService
{
    string GenerateJwtToken(IEnumerable<Claim> claims);
    Task<List<Claim>> GetUserClaimsAsync(AppUser user);
}