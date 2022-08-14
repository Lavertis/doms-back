﻿using DoctorsOffice.Application.Services.Appointment;
using DoctorsOffice.Application.Services.Auth;
using DoctorsOffice.Application.Services.Jwt;
using DoctorsOffice.Application.Services.User;
using Microsoft.Extensions.DependencyInjection;

namespace DoctorsOffice.Application.Services;

public static class ServiceModule
{
    public static void AddServiceModule(this IServiceCollection services)
    {
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IRefreshTokenService, Auth.RefreshTokenService>();
        services.AddScoped<IAppointmentService, Appointment.AppointmentService>();
    }
}