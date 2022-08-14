using DoctorsOffice.Application.CQRS.Queries.Admins.GetAdminById;
using DoctorsOffice.Application.CQRS.Queries.Admins.GetAllAdmins;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DoctorsOffice.API.Controllers;

[ApiController]
[Route("api/admins")]
[Authorize(Roles = RoleTypes.Admin)]
public class AdminController : BaseController
{
    public AdminController(IMediator mediator) : base(mediator)
    {
    }

    /// <summary>
    /// Returns authenticated admin. Only for admins.
    /// </summary>
    [HttpGet("current")]
    public async Task<ActionResult<AdminResponse>> GetAuthenticatedAdminAsync()
        => CreateResponse(await Mediator.Send(new GetAdminByIdQuery(adminId: JwtSubject())));

    /// <summary>
    /// Returns admin with specified id. Only for admins.
    /// </summary>
    [HttpGet("{adminId:guid}")]
    public async Task<ActionResult<AdminResponse>> GetAdminByIdAsync(Guid adminId)
        => CreateResponse(await Mediator.Send(new GetAdminByIdQuery(adminId: adminId)));

    /// <summary>
    /// Returns all admins. Only for admins.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AdminResponse>>> GetAllAdminsAsync()
        => CreateResponse(await Mediator.Send(new GetAllAdminsQuery()));
}