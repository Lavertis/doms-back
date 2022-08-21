using System.Collections.ObjectModel;
using System.Security.Claims;
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
        switch (result.StatusCode)
        {
            case >= 200 and < 300:
                return StatusCode(result.StatusCode, result.Value);
            default:
            {
                if (!string.IsNullOrEmpty(result.ErrorField))
                    return StatusCode(result.StatusCode, CreateFieldError(result.ErrorField, result.Error));
                return StatusCode(result.StatusCode, result.Error);
            }
        }
    }

    private static object CreateFieldError(string fieldName, Error? error)
    {
        var errors = new Dictionary<string, Collection<string?>>();
        errors.Add(fieldName, new Collection<string?> {error?.Message});
        return new {errors};
    }

    protected Guid JwtSubject()
    {
        var subject = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var subjectAsGuid = Guid.Parse(subject);
        return subjectAsGuid;
    }

    protected string JwtRole()
    {
        var role = User.FindFirstValue(ClaimTypes.Role);
        return role;
    }

    protected string? IpAddress()
    {
        if (Request.Headers.ContainsKey("X-Forwarded-For"))
            return Request.Headers["X-Forwarded-For"];
        return HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString();
    }
}