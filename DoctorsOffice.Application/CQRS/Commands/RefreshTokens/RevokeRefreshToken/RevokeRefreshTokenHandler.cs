using DoctorsOffice.Application.Services.RefreshTokens;
using DoctorsOffice.Domain.Utils;
using DoctorsOffice.Infrastructure.Identity;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace DoctorsOffice.Application.CQRS.Commands.RefreshTokens.RevokeRefreshToken;

public class RevokeRefreshTokenHandler : IRequestHandler<RevokeRefreshTokenCommand, HttpResult<Unit>>
{
    private readonly AppUserManager _appUserManager;
    private readonly IRefreshTokenService _refreshTokenService;

    public RevokeRefreshTokenHandler(IRefreshTokenService refreshTokenService, AppUserManager appUserManager)
    {
        _refreshTokenService = refreshTokenService;
        _appUserManager = appUserManager;
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

        if (!refreshToken.IsActive)
        {
            return result
                .WithError(new Error {Message = "Refresh token is already invalidated"})
                .WithStatusCode(StatusCodes.Status400BadRequest);
        }

        _refreshTokenService.RevokeRefreshToken(refreshToken, request.IpAddress, "Revoked without replacement");
        await _appUserManager.UpdateAsync(user);
        return result;
    }
}