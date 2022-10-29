using DoctorsOffice.Application.CQRS.Commands.Users.ConfirmEmail;
using DoctorsOffice.Application.CQRS.Commands.Users.PasswordReset;
using DoctorsOffice.Application.CQRS.Commands.Users.PasswordSet;
using DoctorsOffice.Application.CQRS.Queries.Users.GetAllUsers;
using DoctorsOffice.Application.CQRS.Queries.Users.GetUserById;
using DoctorsOffice.Domain.DTO.Requests;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Enums;
using DoctorsOffice.Domain.Filters;
using DoctorsOffice.Domain.Wrappers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DoctorsOffice.API.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(Roles = Roles.Admin)]
public class UserController : BaseController
{
    public UserController(IMediator mediator) : base(mediator)
    {
    }

    /// <summary>
    /// Returns all base users. Only for admins.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PagedResponse<UserResponse>>> GetAllUsersAsync(
        [FromQuery] PaginationFilter paginationFilter)
        => CreateResponse(await Mediator.Send(new GetAllUsersQuery {PaginationFilter = paginationFilter}));

    /// <summary>
    /// Returns base user by id. Only for admins.
    /// </summary>
    [HttpGet("{userId:guid}")]
    public async Task<ActionResult<UserResponse>> GetUserByIdAsync(Guid userId)
        => CreateResponse(await Mediator.Send(new GetUserByIdQuery(userId)));

    /// <summary>
    /// Confirms user email.
    /// </summary>
    [HttpPost("confirm-email")]
    [AllowAnonymous]
    public async Task<ActionResult<bool>> ConfirmEmailAsync(ConfirmEmailRequest request)
        => CreateResponse(await Mediator.Send(new ConfirmEmailCommand(request)));

    /// <summary>
    /// Sends password reset email.
    /// </summary>
    [HttpPost("password-reset")]
    [AllowAnonymous]
    public async Task<ActionResult<PasswordResetResponse>> PasswordResetAsync(PasswordResetRequest request)
        => CreateResponse(await Mediator.Send(new PasswordResetCommand(request)));

    /// <summary>
    /// Sets user password after reset.
    /// </summary>
    [HttpPost("password-reset/new")]
    [AllowAnonymous]
    public async Task<ActionResult<bool>> PasswordSetAsync(PasswordSetRequest request)
        => CreateResponse(await Mediator.Send(new PasswordSetCommand(request)));
}