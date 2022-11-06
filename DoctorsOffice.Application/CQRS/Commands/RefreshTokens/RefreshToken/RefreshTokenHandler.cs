using DoctorsOffice.Application.Services.Jwt;
using DoctorsOffice.Application.Services.RefreshTokens;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Entities.UserTypes;
using DoctorsOffice.Domain.Utils;
using DoctorsOffice.Infrastructure.Config;
using DoctorsOffice.Infrastructure.Identity;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DoctorsOffice.Application.CQRS.Commands.RefreshTokens.RefreshToken;

using RefreshTokenEntity = Domain.Entities.RefreshToken;

public class RefreshTokenHandler : IRequestHandler<RefreshTokenCommand, HttpResult<AuthenticateResponse>>
{
    private readonly AppUserManager _appUserManager;
    private readonly IJwtService _jwtService;
    private readonly JwtSettings _jwtSettings;
    private readonly IRefreshTokenService _refreshTokenService;

    public RefreshTokenHandler(
        AppUserManager appUserManager,
        IJwtService jwtService,
        IRefreshTokenService refreshTokenService,
        IOptions<JwtSettings> jwtSettings)
    {
        _appUserManager = appUserManager;
        _jwtService = jwtService;
        _refreshTokenService = refreshTokenService;
        _jwtSettings = jwtSettings.Value;
    }

    public async Task<HttpResult<AuthenticateResponse>> Handle(
        RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var result = new HttpResult<AuthenticateResponse>();

        if (request.RefreshToken == null)
        {
            return result
                .WithError(new Error {Message = "Refresh token not found in cookie."})
                .WithStatusCode(StatusCodes.Status400BadRequest);
        }

        var user = await _appUserManager.Users
            .Include(user => user.RefreshTokens)
            .SingleOrDefaultAsync(
                user => user.RefreshTokens.Any(refreshToken => refreshToken.Token == request.RefreshToken),
                cancellationToken: cancellationToken
            );
        if (user is null)
        {
            return result
                .WithError(new Error {Message = "User with specified RefreshToken not found"})
                .WithStatusCode(StatusCodes.Status404NotFound);
        }

        var refreshToken = user.RefreshTokens.Single(x => x.Token == request.RefreshToken);

        if (refreshToken.IsRevoked)
        {
            RevokeDescendantRefreshTokens(
                refreshToken,
                user,
                request.IpAddress,
                $"Attempted reuse of revoked ancestor token: {request.RefreshToken}"
            );
            await _appUserManager.UpdateAsync(user);
        }

        if (!refreshToken.IsActive)
        {
            return result
                .WithError(new Error {Message = "Refresh token is invalid"})
                .WithStatusCode(StatusCodes.Status400BadRequest);
        }

        var newRefreshToken = await RotateRefreshTokenAsync(refreshToken, request.IpAddress, cancellationToken);
        user.RefreshTokens.Add(newRefreshToken);

        _refreshTokenService.RemoveOldRefreshTokens(user);

        await _appUserManager.UpdateAsync(user);

        var claims = await _jwtService.GetUserClaimsAsync(user);
        var jwtToken = _jwtService.GenerateJwtToken(claims);

        return result.WithValue(new AuthenticateResponse
        {
            JwtToken = jwtToken,
            RefreshToken = newRefreshToken.Token,
            CookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.TokenLifetimeInMinutes),
                Secure = true,
                SameSite = SameSiteMode.None,
                IsEssential = true
            }
        });
    }

    private static void RevokeDescendantRefreshTokens(
        RefreshTokenEntity refreshToken, AppUser user, string? ipAddress, string reason)
    {
        if (string.IsNullOrEmpty(refreshToken.ReplacedByToken)) return;

        var childToken = user.RefreshTokens.SingleOrDefault(x => x.Token == refreshToken.ReplacedByToken);
        if (childToken is null) return;

        if (childToken.IsActive)
            RevokeRefreshToken(childToken, ipAddress, reason);
        else
            RevokeDescendantRefreshTokens(childToken, user, ipAddress, reason);
    }

    private static void RevokeRefreshToken(
        RefreshTokenEntity token, string? ipAddress, string? reason = null, string? replacedByToken = null)
    {
        token.RevokedAt = DateTime.UtcNow;
        token.RevokedByIp = ipAddress;
        token.ReasonRevoked = reason;
        token.ReplacedByToken = replacedByToken;
    }

    private async Task<RefreshTokenEntity> RotateRefreshTokenAsync(
        RefreshTokenEntity refreshToken, string? ipAddress, CancellationToken cancellationToken = default)
    {
        var newRefreshToken = await _refreshTokenService.GenerateRefreshTokenAsync(ipAddress, cancellationToken);
        RevokeRefreshToken(refreshToken, ipAddress, "Replaced by new token", newRefreshToken.Token);
        return newRefreshToken;
    }
}