using DoctorsOffice.Application.CQRS.Queries.Admins.GetAdminById;
using DoctorsOffice.Application.CQRS.Queries.Admins.GetAllAdmins;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Enums;
using DoctorsOffice.Domain.Filters;
using DoctorsOffice.Domain.Wrappers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DoctorsOffice.API.Controllers;

[ApiController]
[Route("api/admins")]
[Authorize(Roles = Roles.Admin)]
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
        => CreateResponse(await Mediator.Send(new GetAdminByIdQuery(JwtSubject())));

    /// <summary>
    /// Returns admin with specified id. Only for admins.
    /// </summary>
    [HttpGet("{adminId:guid}")]
    public async Task<ActionResult<AdminResponse>> GetAdminByIdAsync(Guid adminId)
        => CreateResponse(await Mediator.Send(new GetAdminByIdQuery(adminId)));

    /// <summary>
    /// Returns all admins. Only for admins.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PagedResponse<AdminResponse>>> GetAllAdminsAsync(
        [FromQuery] PaginationFilter paginationFilter)
        => CreateResponse(await Mediator.Send(new GetAllAdminsQuery { PaginationFilter = paginationFilter }));
}