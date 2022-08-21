using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DoctorsOffice.Domain.Entities.UserTypes;
using DoctorsOffice.Infrastructure.Config;
using DoctorsOffice.Infrastructure.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace DoctorsOffice.Application.Services.Jwt;

public class JwtService : IJwtService
{
    private readonly AppUserManager _appUserManager;
    private readonly JwtSettings _jwtSettings;

    public JwtService(IOptions<JwtSettings> jwtSettings, AppUserManager appUserManager)
    {
        _jwtSettings = jwtSettings.Value;
        _appUserManager = appUserManager;
    }

    public string GenerateJwtToken(IEnumerable<Claim> claims)
    {
        var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_jwtSettings.SecretKey));

        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.TokenLifetimeInMinutes),
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public async Task<List<Claim>> GetUserClaimsAsync(AppUser user)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Name, user.UserName),
        };

        if (!string.IsNullOrEmpty(user.Email))
            claims.Add(new Claim(JwtRegisteredClaimNames.Email, user.Email));

        var roleNames = await _appUserManager.GetRolesAsync(user);
        claims.AddRange(roleNames.Select(roleName => new Claim(ClaimTypes.Role, roleName)));

        return claims;
    }
}