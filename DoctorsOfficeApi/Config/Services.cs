﻿using DoctorsOfficeApi.Services.AppointmentService;
using DoctorsOfficeApi.Services.AuthService;
using DoctorsOfficeApi.Services.JwtService;
using DoctorsOfficeApi.Services.UserService;

namespace DoctorsOfficeApi.Config;

public static class Services
{
    public static void AddServices(this IServiceCollection services)
    {
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IAppointmentService, AppointmentService>();
    }
}