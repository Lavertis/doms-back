using System.Text;
using DoctorsOffice.Domain.Entities;
using DoctorsOffice.Domain.Entities.UserTypes;
using DoctorsOffice.Infrastructure.Database;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace DoctorsOffice.Infrastructure.Identity;

public static class IdentityModule
{
    public static void AddIdentityModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                var key = Encoding.ASCII.GetBytes(configuration.GetSection("JwtSettings:SecretKey").Value);
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    LifetimeValidator = (_, expires, _, _) => expires >= DateTime.UtcNow // Timezone problem workaround
                };
            });

        services.AddIdentityCore<AppUser>(options =>
            {
                options.Password = new PasswordOptions
                {
                    RequireDigit = false,
                    RequiredLength = 0,
                    RequireLowercase = false,
                    RequireUppercase = false,
                    RequiredUniqueChars = 0,
                    RequireNonAlphanumeric = false,
                };
            })
            .AddRoles<AppRole>()
            .AddRoleManager<AppRoleManager>()
            .AddUserManager<AppUserManager>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();
    }
}