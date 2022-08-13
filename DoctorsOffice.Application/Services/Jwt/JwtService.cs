using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using DoctorsOffice.Domain.Entities;
using DoctorsOffice.Domain.Entities.UserTypes;
using DoctorsOffice.Infrastructure.Config;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace DoctorsOffice.Application.Services.Jwt;

public class JwtService : IJwtService
{
    private readonly AppSettings _appSettings;
    private readonly UserManager<AppUser> _userManager;

    public JwtService(IOptions<AppSettings> appSettings, UserManager<AppUser> userManager)
    {
        _appSettings = appSettings.Value;
        _userManager = userManager;
    }


    public string GenerateJwtToken(IList<Claim> claims)
    {
        var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_appSettings.JwtSecretKey));

        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(15),
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public async Task<RefreshToken> GenerateRefreshTokenAsync(string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        var refreshToken = new RefreshToken
        {
            Token = await GetUniqueToken(cancellationToken),
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow,
            CreatedByIp = ipAddress
        };

        return refreshToken;
    }

    private async Task<string> GetUniqueToken(CancellationToken cancellationToken)
    {
        string token;
        bool tokenIsUnique;

        do
        {
            token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            tokenIsUnique = !await _userManager.Users.AnyAsync(
                u => u.RefreshTokens.Any(t => t.Token == token),
                cancellationToken: cancellationToken
            );
        } while (!tokenIsUnique);

        return token;
    }
}