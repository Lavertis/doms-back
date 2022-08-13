using DoctorsOffice.Domain.Entities;
using DoctorsOffice.Domain.Entities.UserTypes;
using DoctorsOffice.Domain.Exceptions;
using DoctorsOffice.Infrastructure.Config;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DoctorsOffice.Application.Services.Auth;

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
            !x.IsActive && x.CreatedAt.AddDays(_appSettings.RefreshTokenTtlInDays) <= DateTime.UtcNow
        );
    }

    public void RevokeRefreshToken(RefreshToken token, string? ipAddress, string? reason = null,
        string? replacedByToken = null)
    {
        token.RevokedAt = DateTime.UtcNow;
        token.RevokedByIp = ipAddress;
        token.ReasonRevoked = reason;
        token.ReplacedByToken = replacedByToken;
    }
}