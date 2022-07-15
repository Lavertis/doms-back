using DoctorsOfficeApi.Config;
using DoctorsOfficeApi.Entities;
using DoctorsOfficeApi.Entities.UserTypes;
using DoctorsOfficeApi.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DoctorsOfficeApi.Services.AuthService;

public class AuthService : IAuthService
{
    private readonly AppSettings _appSettings;
    private readonly UserManager<AppUser> _userManager;

    public AuthService(UserManager<AppUser> userManager, IOptions<AppSettings> appSettings)
    {
        _userManager = userManager;
        _appSettings = appSettings.Value;
    }

    public async Task<AppUser> GetUserByRefreshTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.Users.SingleOrDefaultAsync(u =>
                u.RefreshTokens.Any(t => t.Token == token), cancellationToken: cancellationToken
        );
        if (user is null)
            throw new NotFoundException("Refresh token is invalid");
        return user;
    }

    public void RemoveOldRefreshTokens(AppUser user)
    {
        // remove old inactive refresh tokens from user based on TTL in app settings
        user.RefreshTokens.RemoveAll(x =>
            !x.IsActive && x.Created.AddDays(_appSettings.RefreshTokenTtl) <= DateTime.UtcNow
        );
    }

    public void RevokeRefreshToken(RefreshToken token, string? ipAddress, string? reason = null, string? replacedByToken = null)
    {
        token.Revoked = DateTime.UtcNow;
        token.RevokedByIp = ipAddress;
        token.ReasonRevoked = reason;
        token.ReplacedByToken = replacedByToken;
    }
}