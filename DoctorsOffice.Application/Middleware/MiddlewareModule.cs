using Microsoft.Extensions.DependencyInjection;

namespace DoctorsOffice.Application.Middleware;

public static class MiddlewareModule
{
    public static void AddMiddlewareModule(this IServiceCollection services)
    {
        services.AddScoped<ExceptionHandlerMiddleware>();
    }
}