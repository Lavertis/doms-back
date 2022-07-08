using DoctorsOfficeApi.Entities.UserTypes;
using DoctorsOfficeApi.Models.Responses;
using DoctorsOfficeApi.Services.AuthService;
using DoctorsOfficeApi.Services.JwtService;
using DoctorsOfficeApi.Services.UserService;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace DoctorsOfficeApi.CQRS.Commands.Authenticate;

public class AuthenticateHandler : IRequestHandler<AuthenticateCommand, AuthenticateResponse>
{
    private readonly IUserService _userService;
    private readonly IJwtService _jwtService;
    private readonly IAuthService _authService;
    private readonly UserManager<AppUser> _userManager;

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

        return new AuthenticateResponse(jwtToken, refreshToken.Token);
    }
}