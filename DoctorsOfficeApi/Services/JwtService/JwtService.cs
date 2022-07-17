using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using DoctorsOfficeApi.Config;
using DoctorsOfficeApi.Entities;
using DoctorsOfficeApi.Entities.UserTypes;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace DoctorsOfficeApi.Services.JwtService;

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
        var secret = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_appSettings.JwtSecretKey));
        var credentials = new SigningCredentials(secret, SecurityAlgorithms.HmacSha512Signature);

        var token = new JwtSecurityToken(
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddMinutes(5),
            signingCredentials: credentials
        );
        var jwt = new JwtSecurityTokenHandler().WriteToken(token);
        return jwt;
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