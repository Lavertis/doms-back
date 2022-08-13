using DoctorsOffice.Application.Services.Auth;
using DoctorsOffice.Application.Services.Jwt;
using DoctorsOffice.Application.Services.User;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Entities.UserTypes;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace DoctorsOffice.Application.CQRS.Commands.Authenticate;

public class AuthenticateHandler : IRequestHandler<AuthenticateCommand, AuthenticateResponse>
{
    private readonly IAuthService _authService;
    private readonly IJwtService _jwtService;
    private readonly UserManager<AppUser> _userManager;
    private readonly IUserService _userService;

    public AuthenticateHandler(
        IUserService userService,
        IJwtService jwtService,
        IAuthService authService,
        UserManager<AppUser> userManager)
    {
        _userService = userService;
        _jwtService = jwtService;
        _authService = authService;
        _userManager = userManager;
    }

    public async Task<AuthenticateResponse> Handle(AuthenticateCommand request, CancellationToken cancellationToken)
    {
        var user = await _userService.GetUserByUserNameAsync(request.UserName);
        var userClaims = await _userService.GetUserRolesAsClaimsAsync(user);
        var jwtToken = _jwtService.GenerateJwtToken(userClaims);
        var refreshToken = await _jwtService.GenerateRefreshTokenAsync(request.IpAddress, cancellationToken);

        _authService.RemoveOldRefreshTokens(user);

        user.RefreshTokens.Add(refreshToken);
        await _userManager.UpdateAsync(user);

        return new AuthenticateResponse
        {
            JwtToken = jwtToken,
            RefreshToken = refreshToken.Token
        };
    }
}