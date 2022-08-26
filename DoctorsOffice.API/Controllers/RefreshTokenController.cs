using DoctorsOffice.Application.CQRS.Commands.RefreshTokens.RevokeRefreshToken;
using DoctorsOffice.Application.CQRS.Queries.RefreshTokens.GetRefreshTokensByUserId;
using DoctorsOffice.Domain.DTO.Requests;
using DoctorsOffice.Domain.Entities;
using DoctorsOffice.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DoctorsOffice.API.Controllers;

[ApiController]
[Route("api/refresh-tokens")]
public class RefreshTokenController : BaseController
{
    public RefreshTokenController(IMediator mediator) : base(mediator)
    {
    }

    /// <summary>
    /// Returns refresh tokens for user by id. Only for admins.
    /// </summary>
    [Authorize(Roles = RoleTypes.Admin)]
    [HttpGet("user/{userId:guid}")]
    public async Task<ActionResult<IEnumerable<RefreshToken>>> GetRefreshTokensByUserIdAsync(Guid userId)
        => CreateResponse(await Mediator.Send(new GetRefreshTokensByUserIdQuery(userId: userId)));

    /// <summary> 
    /// Revokes specified refresh token.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("revoke")]
    public async Task<ActionResult<Unit>> RevokeTokenAsync(RevokeRefreshTokenRequest request)
        => CreateResponse(await Mediator.Send(new RevokeRefreshTokenCommand(request: request, ipAddress: IpAddress())));
}