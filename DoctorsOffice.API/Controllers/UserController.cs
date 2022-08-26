using DoctorsOffice.Application.CQRS.Queries.Users.GetAllUsers;
using DoctorsOffice.Application.CQRS.Queries.Users.GetUserById;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DoctorsOffice.API.Controllers;

[ApiController]
[Route("api/users")]
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
    public async Task<ActionResult<IEnumerable<UserResponse>>> GetAllUsersAsync()
        => CreateResponse(await Mediator.Send(new GetAllUsersQuery()));

    /// <summary>
    /// Returns base user by id. Only for admins.
    /// </summary>
    [HttpGet("{userId:guid}")]
    public async Task<ActionResult<UserResponse>> GetUserByIdAsync(Guid userId)
        => CreateResponse(await Mediator.Send(new GetUserByIdQuery(userId)));
}