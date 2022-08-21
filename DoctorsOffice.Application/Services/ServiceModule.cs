using DoctorsOffice.Application.Services.Appointments;
using DoctorsOffice.Application.Services.Jwt;
using DoctorsOffice.Application.Services.RefreshTokens;
using DoctorsOffice.Application.Services.Users;
using Microsoft.Extensions.DependencyInjection;

namespace DoctorsOffice.Application.Services;

public static class ServiceModule
{
    public static void AddServiceModule(this IServiceCollection services)
    {
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();
        services.AddScoped<IAppointmentService, AppointmentService>();
    }
}