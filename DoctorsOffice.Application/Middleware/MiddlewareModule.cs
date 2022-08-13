using Microsoft.Extensions.DependencyInjection;

namespace DoctorsOffice.Application.Middleware;

public static class MiddlewareModule
{
    public static void AddMiddleware(this IServiceCollection services)
    {
        services.AddScoped<ExceptionHandlerMiddleware>();
    }
}