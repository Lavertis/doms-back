﻿using System.Security.Claims;
using DoctorsOffice.Domain.Enums;
using DoctorsOffice.Domain.Exceptions;
using DoctorsOffice.Domain.Utils;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DoctorsOffice.API.Controllers;

[ApiController]
public abstract class BaseController : ControllerBase
{
    protected readonly IMediator Mediator;

    protected BaseController(IMediator mediator)
    {
        Mediator = mediator;
    }

    protected ActionResult<TValue> CreateResponse<TValue>(HttpResult<TValue> result)
    {
        return result.StatusCode switch
        {
            >= 200 and < 300 => StatusCode(result.StatusCode, result.Value),
            _ => StatusCode(result.StatusCode, result.Error)
        };
    }

    protected Guid JwtSubject()
    {
        var subject = User.FindFirstValue(ClaimTypes.NameIdentifier);
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

    protected string? IpAddress()
    {
        if (Request.Headers.ContainsKey("X-Forwarded-For"))
            return Request.Headers["X-Forwarded-For"];
        return HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString();
    }
}