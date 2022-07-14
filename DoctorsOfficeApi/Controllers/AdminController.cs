using System.Security.Claims;
using DoctorsOfficeApi.CQRS.Queries.GetAdminById;
using DoctorsOfficeApi.CQRS.Queries.GetAllAdmins;
using DoctorsOfficeApi.Models;
using DoctorsOfficeApi.Models.Responses;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DoctorsOfficeApi.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = RoleTypes.Admin)]
[ApiExplorerSettings(GroupName = "Admin")]
public class AdminController : Controller
{
    private readonly IMediator _mediator;

    public AdminController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Returns authenticated admin. Only for admins.
    /// </summary>
    [HttpGet("auth")]
    public async Task<ActionResult<AdminResponse>> GetAuthenticatedAdminAsync()
    {
        var authenticatedAdminId = User.FindFirstValue(ClaimTypes.Sid)!;
        var query = new GetAdminByIdQuery(authenticatedAdminId);
        var adminResponse = await _mediator.Send(query);
        return Ok(adminResponse);
    }

    /// <summary>
    /// Returns admin with specified id. Only for admins.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<AdminResponse>> GetAdminByIdAsync(string id)
    {
        var query = new GetAdminByIdQuery(id);
        var adminResponse = await _mediator.Send(query);
        return Ok(adminResponse);
    }

    /// <summary>
    /// Returns admin with specified id. Only for admins.
    /// </summary>
    [HttpGet("")]
    public async Task<ActionResult<IList<AdminResponse>>> GetAllAdminsAsync()
    {
        var query = new GetAllAdminsQuery();
        var adminResponse = await _mediator.Send(query);
        return Ok(adminResponse);
    }
}