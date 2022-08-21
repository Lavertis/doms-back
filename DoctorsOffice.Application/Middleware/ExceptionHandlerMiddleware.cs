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
            var response = context.Response;
            response.ContentType = "application/json";
            response.StatusCode = StatusCodes.Status500InternalServerError;
            _logger.LogError(exception, "{p0}", exception.Message);
            await response.WriteAsJsonAsync(new {Message = "Internal server error"});
        }
    }
}