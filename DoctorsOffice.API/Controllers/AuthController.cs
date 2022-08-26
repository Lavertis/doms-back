using DoctorsOffice.Application.CQRS.Commands.Authenticate;
using DoctorsOffice.Application.CQRS.Commands.RefreshTokens.RefreshToken;
using DoctorsOffice.Domain.DTO.Requests;
using DoctorsOffice.Domain.DTO.Responses;
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
        => CreateResponse(await Mediator.Send(new AuthenticateCommand(request, IpAddress())));

    /// <summary>
    /// Returns a new pair of access token and refresh token by current refresh token.
    /// </summary>
    [HttpPost("refresh-token")]
    public async Task<ActionResult<AuthenticateResponse>> RefreshTokenAsync(RefreshTokenRequest request)
        => CreateResponse(await Mediator.Send(new RefreshTokenCommand(request, IpAddress())));
}