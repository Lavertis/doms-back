using DoctorsOfficeApi.Entities.UserTypes;
using DoctorsOfficeApi.Exceptions;
using DoctorsOfficeApi.Models.Responses;
using DoctorsOfficeApi.Services.AuthService;
using DoctorsOfficeApi.Services.JwtService;
using DoctorsOfficeApi.Services.UserService;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace DoctorsOfficeApi.CQRS.Commands.RefreshToken;

public class RefreshTokenHandler : IRequestHandler<RefreshTokenCommand, AuthenticateResponse>
{
    private readonly IUserService _userService;
    private readonly UserManager<AppUser> _userManager;
    private readonly IJwtService _jwtService;
    private readonly IAuthService _authService;

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

    public async Task<AuthenticateResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var user = await _authService.GetUserByRefreshTokenAsync(request.RefreshToken, cancellationToken);
        var refreshToken = user.RefreshTokens.Single(x => x.Token == request.RefreshToken);

        if (refreshToken.IsRevoked)
        {
            // revoke all descendant tokens in case this token has been compromised
            RevokeDescendantRefreshTokens(
                refreshToken,
                user,
                request.IpAddress,
                $"Attempted reuse of revoked ancestor token: {request.RefreshToken}"
            );
            await _userManager.UpdateAsync(user);
        }

        if (!refreshToken.IsActive)
            throw new BadRequestException("Refresh token is invalid");

        // replace old refresh token with a new one (rotate token)
        var newRefreshToken = await RotateRefreshTokenAsync(refreshToken, request.IpAddress, cancellationToken);
        user.RefreshTokens.Add(newRefreshToken);

        _authService.RemoveOldRefreshTokens(user);

        await _userManager.UpdateAsync(user);

        var claims = await _userService.GetUserRolesAsClaimsAsync(user);
        var jwtToken = _jwtService.GenerateJwtToken(claims);

        return new AuthenticateResponse(jwtToken, newRefreshToken.Token);
    }

    private void RevokeDescendantRefreshTokens(Entities.RefreshToken refreshToken, AppUser user, string? ipAddress, string reason)
    {
        if (string.IsNullOrEmpty(refreshToken.ReplacedByToken)) return;

        var childToken = user.RefreshTokens.SingleOrDefault(x => x.Token == refreshToken.ReplacedByToken);
        if (childToken is null) return;

        if (childToken.IsActive)
            RevokeRefreshToken(childToken, ipAddress, reason);
        else
            RevokeDescendantRefreshTokens(childToken, user, ipAddress, reason);
    }

    private static void RevokeRefreshToken(Entities.RefreshToken token, string? ipAddress, string? reason = null, string? replacedByToken = null)
    {
        token.Revoked = DateTime.UtcNow;
        token.RevokedByIp = ipAddress;
        token.ReasonRevoked = reason;
        token.ReplacedByToken = replacedByToken;
    }

    private async Task<Entities.RefreshToken> RotateRefreshTokenAsync(Entities.RefreshToken refreshToken, string? ipAddress, CancellationToken cancellationToken = default)
    {
        var newRefreshToken = await _jwtService.GenerateRefreshTokenAsync(ipAddress, cancellationToken);
        RevokeRefreshToken(refreshToken, ipAddress, "Replaced by new token", newRefreshToken.Token);
        return newRefreshToken;
    }
}