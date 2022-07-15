using DoctorsOfficeApi.CQRS.Commands.Authenticate;
using DoctorsOfficeApi.CQRS.Commands.RefreshToken;
using DoctorsOfficeApi.CQRS.Commands.RevokeRefreshToken;
using DoctorsOfficeApi.Models.Requests;
using DoctorsOfficeApi.Models.Responses;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DoctorsOfficeApi.Controllers;

[ApiController]
[Route("api/auth")]
[AllowAnonymous]
[ApiExplorerSettings(GroupName = "Auth")]
public class AuthController : Controller
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Returns a pair of access token and refresh token by username and password.
    /// </summary>
    [HttpPost("authenticate")]
    public async Task<ActionResult<AuthenticateResponse>> AuthenticateAsync(AuthenticateRequest request)
    {
        var command = new AuthenticateCommand(request.UserName, request.Password, IpAddress());
        var response = await _mediator.Send(command);
        return Ok(response);
    }

    /// <summary>
    /// Returns a new pair of access token and refresh token by current refresh token.
    /// </summary>
    [HttpPost("refresh-token")]
    public async Task<ActionResult<AuthenticateResponse>> RefreshTokenAsync(RefreshTokenRequest request)
    {
        var command = new RefreshTokenCommand(request.RefreshToken, IpAddress());
        var response = await _mediator.Send(command);
        return Ok(response);
    }

    /// <summary> 
    /// Revokes specified refresh token.
    /// </summary>
    [HttpPost("revoke-token")]
    public async Task<IActionResult> RevokeTokenAsync(RevokeRefreshTokenRequest request)
    {
        var command = new RevokeRefreshTokenCommand(request.RefreshToken, IpAddress());
        await _mediator.Send(command);
        return Ok(new { message = "Token revoked" });
    }

    private string? IpAddress()
    {
        // get source ip address for the current request
        if (Request.Headers.ContainsKey("X-Forwarded-For"))
            return Request.Headers["X-Forwarded-For"];
        return HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? null;
    }
}