using DoctorsOfficeApi.Middleware;

namespace DoctorsOfficeApi.Modules;

public static class Middleware
{
    public static void AddMiddleware(this IServiceCollection services)
    {
        services.AddScoped<ExceptionHandlerMiddleware>();
    }
}