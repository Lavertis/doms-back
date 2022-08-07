using System.Security.Claims;
using DoctorsOfficeApi.Exceptions;
using DoctorsOfficeApi.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DoctorsOfficeApi.Controllers;

[ApiController]
public abstract class BaseController : ControllerBase
{
    protected readonly IMediator Mediator;

    protected BaseController(IMediator mediator)
    {
        Mediator = mediator;
    }

    protected Guid JwtSubject()
    {
        var subject = User.FindFirstValue(ClaimTypes.Sid);
        if (subject is null)
            throw new BadRequestException("Could not get subject from Jwt token");

        if (!Guid.TryParse(subject, out var subjectAsGuid))
            throw new BadRequestException("Jwt subject is not a valid Guid");

        return subjectAsGuid;
    }

    protected string JwtRole()
    {
        var role = User.FindFirstValue(ClaimTypes.Role);
        if (role is null)
            throw new BadRequestException("Jwt does not have the Role claim");
        if (role is not (RoleTypes.Patient or RoleTypes.Doctor or RoleTypes.Admin))
            throw new BadRequestException("Role claim contains wrong value");

        return role;
    }
}