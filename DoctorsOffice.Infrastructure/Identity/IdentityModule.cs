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

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];

                        // If the request is for our hub...
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) &&
                            (path.StartsWithSegments("/api/chat")))
                        {
                            // Read the token out of the query string
                            context.Token = accessToken;
                        }

                        return Task.CompletedTask;
                    }
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