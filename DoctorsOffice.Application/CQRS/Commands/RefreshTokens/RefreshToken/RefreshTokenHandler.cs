using DoctorsOffice.Application.Services.Auth;
using DoctorsOffice.Application.Services.Jwt;
using DoctorsOffice.Application.Services.User;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Entities.UserTypes;
using DoctorsOffice.Domain.Utils;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace DoctorsOffice.Application.CQRS.Commands.RefreshTokens.RefreshToken;

using RefreshTokenEntity = Domain.Entities.RefreshToken;

public class RefreshTokenHandler : IRequestHandler<RefreshTokenCommand, HttpResult<AuthenticateResponse>>
{
    private readonly IAuthService _authService;
    private readonly IJwtService _jwtService;
    private readonly UserManager<AppUser> _userManager;
    private readonly IUserService _userService;

    public RefreshTokenHandler(
        UserManager<AppUser> userManager,
        IJwtService jwtService,
        IUserService userService, IAuthService authService)
    {
        _userManager = userManager;
        _jwtService = jwtService;
        _userService = userService;
        _authService = authService;
    }

    public async Task<HttpResult<AuthenticateResponse>> Handle(
        RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var result = new HttpResult<AuthenticateResponse>();

        var user = await _authService.GetUserByRefreshTokenAsync(request.RefreshToken, cancellationToken);
        var refreshToken = user.RefreshTokens.Single(x => x.Token == request.RefreshToken);

        if (refreshToken.IsRevoked)
        {
            RevokeDescendantRefreshTokens(
                refreshToken,
                user,
                request.IpAddress,
                $"Attempted reuse of revoked ancestor token: {request.RefreshToken}"
            );
            await _userManager.UpdateAsync(user);
        }

        if (!refreshToken.IsActive)
        {
            return result
                .WithError(new Error {Message = "Refresh token is invalid"})
                .WithStatusCode(StatusCodes.Status400BadRequest);
        }

        var newRefreshToken = await RotateRefreshTokenAsync(refreshToken, request.IpAddress, cancellationToken);
        user.RefreshTokens.Add(newRefreshToken);

        _authService.RemoveOldRefreshTokens(user);

        await _userManager.UpdateAsync(user);

        var claims = await _userService.GetUserRolesAsClaimsAsync(user);
        var jwtToken = _jwtService.GenerateJwtToken(claims);

        return result.WithValue(new AuthenticateResponse
        {
            JwtToken = jwtToken,
            RefreshToken = newRefreshToken.Token
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
        var newRefreshToken = await _jwtService.GenerateRefreshTokenAsync(ipAddress, cancellationToken);
        RevokeRefreshToken(refreshToken, ipAddress, "Replaced by new token", newRefreshToken.Token);
        return newRefreshToken;
    }
}