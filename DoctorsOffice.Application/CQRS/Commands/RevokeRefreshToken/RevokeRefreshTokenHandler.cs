using DoctorsOffice.Application.Services.Auth;
using DoctorsOffice.Domain.Entities.UserTypes;
using DoctorsOffice.Domain.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace DoctorsOfficeApi.CQRS.Commands.RevokeRefreshToken;

public class RevokeRefreshTokenHandler : IRequestHandler<RevokeRefreshTokenCommand, Unit>
{
    private readonly IAuthService _authService;
    private readonly UserManager<AppUser> _userManager;

    public RevokeRefreshTokenHandler(IAuthService authService, UserManager<AppUser> userManager)
    {
        _authService = authService;
        _userManager = userManager;
    }

    public async Task<Unit> Handle(RevokeRefreshTokenCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.RefreshToken))
            throw new BadRequestException("Refresh token is null");

        var user = await _authService.GetUserByRefreshTokenAsync(request.RefreshToken, cancellationToken);
        var refreshToken = user.RefreshTokens.Single(x => x.Token == request.RefreshToken);

        if (!refreshToken.IsActive)
            throw new BadRequestException("Refresh token is already invalidated");

        _authService.RevokeRefreshToken(refreshToken, request.IpAddress, "Revoked without replacement");
        await _userManager.UpdateAsync(user);
        return await Unit.Task;
    }
}