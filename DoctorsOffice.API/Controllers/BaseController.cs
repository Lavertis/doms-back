using System.Security.Claims;
using DoctorsOffice.Domain.Enums;
using DoctorsOffice.Domain.Utils;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using FileResult = DoctorsOffice.Domain.Utils.FileResult;

namespace DoctorsOffice.API.Controllers;

[ApiController]
[Produces(ContentTypes.ApplicationJson)]
[Consumes(ContentTypes.ApplicationJson)]
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
            _ when result.IsError => StatusCode(result.StatusCode, new {Error = result.Error?.Message}),
            _ when result.HasValidationErrors => StatusCode(result.StatusCode, new {Errors = result.ValidationErrors}),
            _ => throw new Exception("Failed to created response")
        };
    }

    protected ActionResult<MemoryStream> CreateResponse(HttpResult<FileResult> result)
    {
        return result.StatusCode switch
        {
            >= 200 and < 300 => CreateFileStreamResult(result.Value!),
            _ when result.IsError => StatusCode(result.StatusCode, result.Error),
            _ when result.HasValidationErrors => StatusCode(result.StatusCode, new {Errors = result.ValidationErrors}),
            _ => throw new Exception("Failed to created response")
        };
    }

    private FileStreamResult CreateFileStreamResult(FileResult fileResult)
    {
        AddExposedHeader("Content-Disposition");
        return new FileStreamResult(fileResult.Stream, fileResult.ContentType)
        {
            FileDownloadName = fileResult.FileName
        };
    }

    private void AddExposedHeader(string header)
    {
        var exposedHeaders = Response.Headers["Access-Control-Expose-Headers"];
        if (string.IsNullOrEmpty(exposedHeaders))
            exposedHeaders = header;
        else
            exposedHeaders += $",{header}";
        Response.Headers["Access-Control-Expose-Headers"] = exposedHeaders;
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