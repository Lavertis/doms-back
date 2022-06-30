using DoctorsOfficeApi.Data;
using DoctorsOfficeApi.Entities;
using DoctorsOfficeApi.Entities.UserTypes;
using DoctorsOfficeApi.Exceptions;
using DoctorsOfficeApi.Models.Requests;
using DoctorsOfficeApi.Models.Responses;
using DoctorsOfficeApi.Services.JwtService;
using DoctorsOfficeApi.Services.UserService;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DoctorsOfficeApi.Services.AuthService;

public class AuthService : IAuthService
{
    private readonly AppSettings _appSettings;
    private readonly IJwtService _jwtService;
    private readonly UserManager<AppUser> _userManager;
    private readonly IUserService _userService;

    public AuthService(UserManager<AppUser> userManager, IJwtService jwtService, IUserService userService, IOptions<AppSettings> appSettings)
    {
        _userManager = userManager;
        _jwtService = jwtService;
        _userService = userService;
        _appSettings = appSettings.Value;
    }

    
    public async Task<AuthenticateResponse> AuthenticateAsync(AuthenticateRequest request, string? ipAddress, CancellationToken cancellationToken = default)
    {
        var user = await _userService.GetUserByUserNameAsync(request.UserName);
        var userClaims = await _userService.GetUserRolesAsClaimsAsync(user);
        var jwtToken = _jwtService.GenerateJwtToken(userClaims);
        var refreshToken = await _jwtService.GenerateRefreshTokenAsync(ipAddress, cancellationToken);

        RemoveOldRefreshTokens(user);

        user.RefreshTokens.Add(refreshToken);
        await _userManager.UpdateAsync(user);

        return new AuthenticateResponse(jwtToken, refreshToken.Token);
    }

    public async Task<AuthenticateResponse> RefreshTokenAsync(string token, string? ipAddress)
    {
        var user = await GetUserByRefreshTokenAsync(token);
        var refreshToken = user.RefreshTokens.Single(x => x.Token == token);

        if (refreshToken.IsRevoked)
        {
            // revoke all descendant tokens in case this token has been compromised
            RevokeDescendantRefreshTokens(
                refreshToken,
                user,
                ipAddress,
                $"Attempted reuse of revoked ancestor token: {token}"
            );
            await _userManager.UpdateAsync(user);
        }

        if (!refreshToken.IsActive)
            throw new BadRequestException("Refresh token is invalid");

        // replace old refresh token with a new one (rotate token)
        var newRefreshToken = await RotateRefreshTokenAsync(refreshToken, ipAddress);
        user.RefreshTokens.Add(newRefreshToken);
        
        RemoveOldRefreshTokens(user);
        
        await _userManager.UpdateAsync(user);
        
        var claims = await _userService.GetUserRolesAsClaimsAsync(user);
        var jwtToken = _jwtService.GenerateJwtToken(claims);

        return new AuthenticateResponse(jwtToken, newRefreshToken.Token);
    }

    public async Task<bool> RevokeRefreshTokenAsync(string token, string? ipAddress)
    {
        if (string.IsNullOrEmpty(token))
            throw new BadRequestException("Refresh token is null");

        var user = await GetUserByRefreshTokenAsync(token);
        var refreshToken = user.RefreshTokens.Single(x => x.Token == token);

        if (!refreshToken.IsActive)
            throw new BadRequestException("Refresh token is already invalidated");
        
        RevokeRefreshToken(refreshToken, ipAddress, "Revoked without replacement");
        var identityResult = await _userManager.UpdateAsync(user);
        return identityResult.Succeeded;
    }
    
    private void RemoveOldRefreshTokens(AppUser user)
    {
        // remove old inactive refresh tokens from user based on TTL in app settings
        user.RefreshTokens.RemoveAll(x =>
            !x.IsActive && x.Created.AddDays(_appSettings.RefreshTokenTtl) <= DateTime.UtcNow
        );
    }
    
    private async Task<AppUser> GetUserByRefreshTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.Users.SingleOrDefaultAsync(u =>
                u.RefreshTokens.Any(t => t.Token == token), cancellationToken: cancellationToken
        );
        if (user == null)
            throw new NotFoundException("Refresh token is invalid");
        return user;
    }
    
    private async Task<RefreshToken> RotateRefreshTokenAsync(RefreshToken refreshToken, string? ipAddress, CancellationToken cancellationToken = default)
    {
        var newRefreshToken = await _jwtService.GenerateRefreshTokenAsync(ipAddress, cancellationToken);
        RevokeRefreshToken(refreshToken, ipAddress, "Replaced by new token", newRefreshToken.Token);
        return newRefreshToken;
    }
    
    private void RevokeDescendantRefreshTokens(RefreshToken refreshToken, AppUser user, string? ipAddress, string reason)
    {
        if (string.IsNullOrEmpty(refreshToken.ReplacedByToken)) return;
        
        var childToken = user.RefreshTokens.SingleOrDefault(x => x.Token == refreshToken.ReplacedByToken);
        if (childToken == null) return;

        if (childToken.IsActive)
            RevokeRefreshToken(childToken, ipAddress, reason);
        else
            RevokeDescendantRefreshTokens(childToken, user, ipAddress, reason);
    }

    private static void RevokeRefreshToken(RefreshToken token, string? ipAddress, string? reason = null, string? replacedByToken = null)
    {
        token.Revoked = DateTime.UtcNow;
        token.RevokedByIp = ipAddress;
        token.ReasonRevoked = reason;
        token.ReplacedByToken = replacedByToken;
    }
}