using DoctorsOffice.Application.CQRS.Queries.GetAdminById;
using DoctorsOffice.Application.CQRS.Queries.GetAllAdmins;
using DoctorsOffice.Domain.Enums;
using DoctorsOfficeApi.Models.Responses;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DoctorsOffice.API.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = RoleTypes.Admin)]
public class AdminController : BaseController
{
    public AdminController(IMediator mediator) : base(mediator)
    {
    }

    /// <summary>
    /// Returns authenticated admin. Only for admins.
    /// </summary>
    [HttpGet("auth")]
    public async Task<ActionResult<AdminResponse>> GetAuthenticatedAdminAsync()
        => Ok(await Mediator.Send(new GetAdminByIdQuery(adminId: JwtSubject())));

    /// <summary>
    /// Returns admin with specified id. Only for admins.
    /// </summary>
    [HttpGet("{adminId:guid}")]
    public async Task<ActionResult<AdminResponse>> GetAdminByIdAsync(Guid adminId)
        => Ok(await Mediator.Send(new GetAdminByIdQuery(adminId: adminId)));

    /// <summary>
    /// Returns all admins. Only for admins.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IList<AdminResponse>>> GetAllAdminsAsync()
        => Ok(await Mediator.Send(new GetAllAdminsQuery()));
}