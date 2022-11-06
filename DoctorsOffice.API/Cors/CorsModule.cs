using DoctorsOffice.Infrastructure.Config;

namespace DoctorsOffice.API.Cors;

public static class CorsModule
{
    public static void UseCorsModule(this IApplicationBuilder app, CorsSettings corsSettings)
    {
        app.UseCors(options => options
            .AllowAnyHeader()
            .AllowAnyMethod()
            .WithOrigins(corsSettings.AllowedOrigins)
            .AllowCredentials()
        );
    }
}