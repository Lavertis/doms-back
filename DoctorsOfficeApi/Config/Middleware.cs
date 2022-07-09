using DoctorsOfficeApi.Middleware;

namespace DoctorsOfficeApi.Config;

public static class Middleware
{
    public static void AddMiddleware(this IServiceCollection services)
    {
        services.AddScoped<ExceptionHandlerMiddleware>();
    }
}