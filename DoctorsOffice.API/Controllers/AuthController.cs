using DoctorsOffice.Application.CQRS.Commands.Authenticate;
using DoctorsOffice.Application.CQRS.Commands.RefreshTokens.RefreshToken;
using DoctorsOffice.Domain.DTO.Requests;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DoctorsOffice.API.Controllers;

[ApiController]
[Route("api/auth")]
[AllowAnonymous]
public class AuthController : BaseController
{
    public AuthController(IMediator mediator) : base(mediator)
    {
    }

    /// <summary>
    /// Returns a pair of access token and refresh token by username and password.
    /// </summary>
    [HttpPost("authenticate")]
    public async Task<ActionResult<AuthenticateResponse>> AuthenticateAsync(AuthenticateRequest request)
    {
        var result = await Mediator.Send(new AuthenticateCommand(request, IpAddress()));
        if (result.IsError || result.HasValidationErrors || result.Value == null)
            return CreateResponse(result);
        SetRefreshTokenCookie(result.Value.RefreshToken, result.Value.CookieOptions);
        return CreateResponse(result);
    }

    /// <summary>
    /// Returns a new pair of access token and refresh token by current refresh token.
    /// </summary>
    [HttpPost("refresh-token")]
    public async Task<ActionResult<AuthenticateResponse>> RefreshTokenAsync()
    {
        var refreshToken = Request.Cookies[Cookies.RefreshToken];
        var result = await Mediator.Send(new RefreshTokenCommand
        {
            RefreshToken = refreshToken,
            IpAddress = IpAddress()
        });
        if (result.IsError || result.HasValidationErrors || result.Value == null)
            return CreateResponse(result);
        SetRefreshTokenCookie(result.Value.RefreshToken, result.Value.CookieOptions);
        return CreateResponse(result);
    }

    /// <summary>
    /// Removes the refresh token cookie.
    /// </summary>
    [HttpPost("logout")]
    public ActionResult<bool> Logout()
    {
        RemoveRefreshTokenCookie();
        return Ok(true);
    }

    private void SetRefreshTokenCookie(string token, CookieOptions cookieOptions)
    {
        Response.Cookies.Append(Cookies.RefreshToken, token, cookieOptions);
    }

    private void RemoveRefreshTokenCookie()
    {
        Response.Cookies.Delete(Cookies.RefreshToken, new CookieOptions
        {
            HttpOnly = true,
            Expires = DateTime.UtcNow.AddYears(-1),
            Secure = true,
            SameSite = SameSiteMode.None,
            IsEssential = true
        });
    }
}