using DoctorsOfficeApi.CQRS.Queries.GetAllUsers;
using DoctorsOfficeApi.CQRS.Queries.GetRefreshTokensByUserId;
using DoctorsOfficeApi.CQRS.Queries.GetUserById;
using DoctorsOfficeApi.Entities;
using DoctorsOfficeApi.Models;
using DoctorsOfficeApi.Models.Responses;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DoctorsOfficeApi.Controllers;

[ApiController]
[Route("api/user")]
[Authorize(Roles = RoleTypes.Admin)]
[ApiExplorerSettings(GroupName = "User")]
public class UserController : Controller
{
    private readonly IMediator _mediator;

    public UserController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Returns all base users. Only for admins.
    /// </summary>
    [HttpGet("")]
    public async Task<ActionResult<IList<UserResponse>>> GetAllUsersAsync()
    {
        var query = new GetAllUsersQuery();
        var userResponses = await _mediator.Send(query);
        return Ok(userResponses);
    }

    /// <summary>
    /// Returns base user by id. Only for admins.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<IList<UserResponse>>> GetUserByIdAsync(string id)
    {
        var query = new GetUserByIdQuery(id);
        var userResponse = await _mediator.Send(query);
        return Ok(userResponse);
    }

    /// <summary>
    /// Returns refresh token for user by id. Only for admins.
    /// </summary>
    [HttpGet("{id}/refresh-tokens")]
    public async Task<ActionResult<IList<RefreshToken>>> GetRefreshTokensByUserId(string id)
    {
        var query = new GetRefreshTokensByUserIdQuery(id);
        var refreshTokens = await _mediator.Send(query);
        return Ok(refreshTokens);
    }
}