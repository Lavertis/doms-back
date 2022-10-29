using System.Text;
using DoctorsOffice.Domain.Entities;
using DoctorsOffice.Domain.Entities.UserTypes;
using DoctorsOffice.Infrastructure.Database;
using DoctorsOffice.Infrastructure.Identity.TokenProviders;
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
                var key = Encoding.ASCII.GetBytes(configuration.GetSection("Jwt:SecretKey").Value);
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
                options.SignIn.RequireConfirmedEmail = true;
                options.Tokens.EmailConfirmationTokenProvider = "email_confirmation";
                options.Tokens.PasswordResetTokenProvider = "password_reset";
            })
            .AddRoles<AppRole>()
            .AddRoleManager<AppRoleManager>()
            .AddUserManager<AppUserManager>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders()
            .AddTokenProvider<EmailConfirmationTokenProvider<AppUser>>("email_confirmation")
            .AddTokenProvider<PasswordResetTokenProvider<AppUser>>("password_reset");

        var emailConfirmationTokenLifeSpanInHours =
            int.Parse(configuration.GetSection("Identity:EmailConfirmationTokenLifeSpanInHours").Value);
        services.Configure<EmailConfirmationTokenProviderOptions>(opt =>
            opt.TokenLifespan = TimeSpan.FromHours(emailConfirmationTokenLifeSpanInHours));

        var passwordResetTokenLifeSpanInHours =
            int.Parse(configuration.GetSection("Identity:PasswordResetTokenLifeSpanInHours").Value);
        services.Configure<PasswordResetTokenProviderOptions>(opt =>
            opt.TokenLifespan = TimeSpan.FromHours(passwordResetTokenLifeSpanInHours));
    }
}