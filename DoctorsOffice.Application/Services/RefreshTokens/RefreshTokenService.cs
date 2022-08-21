using System.Security.Cryptography;
using DoctorsOffice.Domain.Entities;
using DoctorsOffice.Domain.Entities.UserTypes;
using DoctorsOffice.Infrastructure.Config;
using DoctorsOffice.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DoctorsOffice.Application.Services.RefreshTokens;

public class RefreshTokenService : IRefreshTokenService
{
    private readonly AppUserManager _appUserManager;
    private readonly JwtSettings _jwtSettings;

    public RefreshTokenService(AppUserManager appUserManager, IOptions<JwtSettings> jwtSettings)
    {
        _appUserManager = appUserManager;
        _jwtSettings = jwtSettings.Value;
    }

    public async Task<RefreshToken> GenerateRefreshTokenAsync(string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        var refreshToken = new RefreshToken
        {
            Token = await GetUniqueToken(cancellationToken),
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenLifetimeInDays),
            CreatedAt = DateTime.UtcNow,
            CreatedByIp = ipAddress
        };

        return refreshToken;
    }

    public void RemoveOldRefreshTokens(AppUser user)
    {
        user.RefreshTokens.RemoveAll(x =>
            !x.IsActive && x.CreatedAt.AddDays(_jwtSettings.RefreshTokenTtlInDays) <= DateTime.UtcNow);
    }

    public void RevokeRefreshToken(RefreshToken token, string? ipAddress, string? reason = null,
        string? replacedByToken = null)
    {
        token.RevokedAt = DateTime.UtcNow;
        token.RevokedByIp = ipAddress;
        token.ReasonRevoked = reason;
        token.ReplacedByToken = replacedByToken;
    }

    private async Task<string> GetUniqueToken(CancellationToken cancellationToken)
    {
        string token;
        bool tokenIsUnique;

        do
        {
            token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            tokenIsUnique = !await _appUserManager.Users.AnyAsync(
                u => u.RefreshTokens.Any(t => t.Token == token),
                cancellationToken: cancellationToken
            );
        } while (!tokenIsUnique);

        return token;
    }
}