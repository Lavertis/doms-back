using DoctorsOffice.Domain.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace DoctorsOffice.Application.Middleware;

public class ExceptionHandlerMiddleware : IMiddleware
{
    private readonly ILogger<ExceptionHandlerMiddleware> _logger;

    public ExceptionHandlerMiddleware(ILogger<ExceptionHandlerMiddleware> logger)
    {
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            // TODO handle AppExceptions in different catch and use Error property to get message
            var response = context.Response;
            response.ContentType = "application/json";

            response.StatusCode = exception switch
            {
                BadRequestException => StatusCodes.Status400BadRequest,
                NotFoundException => StatusCodes.Status404NotFound,
                ForbiddenException => StatusCodes.Status403Forbidden,
                ConflictException => StatusCodes.Status409Conflict,
                _ => StatusCodes.Status500InternalServerError
            };

            if (response.StatusCode != StatusCodes.Status500InternalServerError)
            {
                await response.WriteAsJsonAsync(new {exception.Message});
            }
            else
            {
                _logger.LogError(exception, "{p0}", exception.Message);
                await response.WriteAsJsonAsync(new {Message = "Internal server error"});
            }
        }
    }
}