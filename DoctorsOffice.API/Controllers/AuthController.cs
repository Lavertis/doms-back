using DoctorsOffice.Application.CQRS.Commands.Authenticate;
using DoctorsOffice.Application.CQRS.Commands.RefreshToken;
using DoctorsOffice.Domain.DTO.Requests;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOfficeApi.CQRS.Commands.RevokeRefreshToken;
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
        => Ok(await Mediator.Send(new AuthenticateCommand(request: request, ipAddress: IpAddress())));

    /// <summary>
    /// Returns a new pair of access token and refresh token by current refresh token.
    /// </summary>
    [HttpPost("refresh-token")]
    public async Task<ActionResult<AuthenticateResponse>> RefreshTokenAsync(RefreshTokenRequest request)
        => Ok(await Mediator.Send(new RefreshTokenCommand(request: request, ipAddress: IpAddress())));

    /// <summary> 
    /// Revokes specified refresh token.
    /// </summary>
    [HttpPost("revoke-token")]
    public async Task<IActionResult> RevokeTokenAsync(RevokeRefreshTokenRequest request)
        => Ok(await Mediator.Send(new RevokeRefreshTokenCommand(request: request, ipAddress: IpAddress())));

    private string? IpAddress()
    {
        // get source ip address for the current request
        if (Request.Headers.ContainsKey("X-Forwarded-For"))
            return Request.Headers["X-Forwarded-For"];
        return HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString();
    }
}