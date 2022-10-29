using DoctorsOffice.Application.Services.Jwt;
using DoctorsOffice.Application.Services.RefreshTokens;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Utils;
using DoctorsOffice.Infrastructure.Identity;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace DoctorsOffice.Application.CQRS.Commands.Authenticate;

public class AuthenticateHandler : IRequestHandler<AuthenticateCommand, HttpResult<AuthenticateResponse>>
{
    private readonly AppUserManager _appUserManager;
    private readonly IJwtService _jwtService;
    private readonly IRefreshTokenService _refreshTokenService;

    public AuthenticateHandler(
        IJwtService jwtService,
        IRefreshTokenService refreshTokenService,
        AppUserManager appUserManager)
    {
        _jwtService = jwtService;
        _refreshTokenService = refreshTokenService;
        _appUserManager = appUserManager;
    }

    public async Task<HttpResult<AuthenticateResponse>> Handle(
        AuthenticateCommand request, CancellationToken cancellationToken)
    {
        var result = new HttpResult<AuthenticateResponse>();

        var findByNameResult = await _appUserManager.FindByNameAsync(request.Login);
        var findByEmailResult = await _appUserManager.FindByEmailAsync(request.Login);
        if (findByNameResult.IsError && findByEmailResult.IsError &&
            findByNameResult.Value == null && findByEmailResult.Value == null)
        {
            return result
                .WithError(InvalidCredentialsError())
                .WithStatusCode(StatusCodes.Status404NotFound);
        }

        var user = findByNameResult.Value ?? findByEmailResult.Value;
        var validatePasswordResult = await _appUserManager.ValidatePasswordAsync(user!.UserName, request.Password);
        if (validatePasswordResult.IsError || !validatePasswordResult.Value)
        {
            return result
                .WithError(InvalidCredentialsError())
                .WithStatusCode(StatusCodes.Status404NotFound);
        }

        var emailIsConfirmed = await _appUserManager.IsEmailConfirmedAsync(user);
        if (!emailIsConfirmed)
        {
            return result
                .WithError(new Error {Message = "Email address not confirmed"})
                .WithStatusCode(StatusCodes.Status400BadRequest);
        }

        var userClaims = await _jwtService.GetUserClaimsAsync(user);
        var jwtToken = _jwtService.GenerateJwtToken(userClaims);
        var refreshToken = await _refreshTokenService.GenerateRefreshTokenAsync(request.IpAddress, cancellationToken);

        _refreshTokenService.RemoveOldRefreshTokens(user);

        user.RefreshTokens.Add(refreshToken);
        await _appUserManager.UpdateAsync(user);

        return result.WithValue(new AuthenticateResponse
        {
            JwtToken = jwtToken,
            RefreshToken = refreshToken.Token
        });
    }

    private static Error InvalidCredentialsError() => new() {Message = "Invalid credentials."};
}