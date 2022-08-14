using DoctorsOffice.Application.Services.Auth;
using DoctorsOffice.Domain.Entities.UserTypes;
using DoctorsOffice.Domain.Utils;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace DoctorsOffice.Application.CQRS.Commands.RefreshTokens.RevokeRefreshToken;

public class RevokeRefreshTokenHandler : IRequestHandler<RevokeRefreshTokenCommand, HttpResult<Unit>>
{
    private readonly IAuthService _authService;
    private readonly UserManager<AppUser> _userManager;

    public RevokeRefreshTokenHandler(IAuthService authService, UserManager<AppUser> userManager)
    {
        _authService = authService;
        _userManager = userManager;
    }

    public async Task<HttpResult<Unit>> Handle(RevokeRefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var result = new HttpResult<Unit>();

        if (string.IsNullOrEmpty(request.RefreshToken))
        {
            return result
                .WithError(new Error {Message = "Refresh token is null or empty"})
                .WithStatusCode(StatusCodes.Status400BadRequest);
        }

        var user = await _authService.GetUserByRefreshTokenAsync(request.RefreshToken, cancellationToken);
        var refreshToken = user.RefreshTokens.Single(x => x.Token == request.RefreshToken);

        if (!refreshToken.IsActive)
        {
            return result
                .WithError(new Error {Message = "Refresh token is already invalidated"})
                .WithStatusCode(StatusCodes.Status400BadRequest);
        }

        _authService.RevokeRefreshToken(refreshToken, request.IpAddress, "Revoked without replacement");
        await _userManager.UpdateAsync(user);
        return result;
    }
}