using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DoctorsOffice.Infrastructure.Database;

public static class DatabaseModule
{
    public static void AddDatabaseModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(GetConnectionString(configuration)));
    }

    private static string GetConnectionString(IConfiguration configuration)
    {
        var aspnetcoreEnvironment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        return aspnetcoreEnvironment switch
        {
            "Development" => configuration.GetConnectionString("AppDb"),
            "Staging" => GetHerokuConnectionString()!,
            _ => throw new Exception("Wrong ASPNETCORE_ENVIRONMENT value")
        };
    }

    private static string? GetHerokuConnectionString()
    {
        var connectionUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
        if (connectionUrl is null)
            return null;

        var databaseUri = new Uri(connectionUrl);
        var db = databaseUri.LocalPath.TrimStart('/');
        var userInfo = databaseUri.UserInfo.Split(':', StringSplitOptions.RemoveEmptyEntries);
        return $"UserID={userInfo[0]};Password={userInfo[1]};Host={databaseUri.Host};Port={databaseUri.Port};" +
               $"Database={db};Pooling=true;SSLMode=Require;TrustServerCertificate=True;";
    }
}