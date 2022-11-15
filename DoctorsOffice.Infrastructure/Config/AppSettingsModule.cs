using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DoctorsOffice.Infrastructure.Config;

public static class JwtSettingsModule
{
    public static void AddAppSettingsModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection("Jwt"));
        services.Configure<SendGridTemplateSettings>(configuration.GetSection("SendGridTemplates"));
        services.Configure<UrlSettings>(configuration.GetSection("Url"));
        services.Configure<IdentitySettings>(configuration.GetSection("Identity"));
        services.Configure<QuickButtonSettings>(configuration.GetSection("QuickButton"));
    }
}