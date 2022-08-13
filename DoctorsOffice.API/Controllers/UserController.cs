using DoctorsOffice.Application.CQRS.Queries.GetAllUsers;
using DoctorsOffice.Application.CQRS.Queries.GetRefreshTokensByUserId;
using DoctorsOffice.Application.CQRS.Queries.GetUserById;
using DoctorsOffice.Domain.Entities;
using DoctorsOffice.Domain.Enums;
using DoctorsOfficeApi.Models.Responses;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DoctorsOffice.API.Controllers;

[ApiController]
[Route("api/user")]
[Authorize(Roles = RoleTypes.Admin)]
public class UserController : BaseController
{
    public UserController(IMediator mediator) : base(mediator)
    {
    }

    /// <summary>
    /// Returns all base users. Only for admins.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IList<UserResponse>>> GetAllUsersAsync()
        => Ok(await Mediator.Send(new GetAllUsersQuery()));

    /// <summary>
    /// Returns base user by id. Only for admins.
    /// </summary>
    [HttpGet("{userId:guid}")]
    public async Task<ActionResult<IList<UserResponse>>> GetUserByIdAsync(Guid userId)
        => Ok(await Mediator.Send(new GetUserByIdQuery(userId: userId)));

    /// <summary>
    /// Returns refresh token for user by id. Only for admins.
    /// </summary>
    [HttpGet("{userId:guid}/refresh-tokens")]
    public async Task<ActionResult<IList<RefreshToken>>> GetRefreshTokensByUserId(Guid userId)
        => Ok(await Mediator.Send(new GetRefreshTokensByUserIdQuery(userId: userId)));
}