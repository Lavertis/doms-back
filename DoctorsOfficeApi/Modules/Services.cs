using DoctorsOfficeApi.Services.AdminService;
using DoctorsOfficeApi.Services.AppointmentService;
using DoctorsOfficeApi.Services.AuthService;
using DoctorsOfficeApi.Services.DoctorService;
using DoctorsOfficeApi.Services.JwtService;
using DoctorsOfficeApi.Services.PatientService;
using DoctorsOfficeApi.Services.UserService;

namespace DoctorsOfficeApi.Modules;

public static class Services
{
    public static void AddServices(this IServiceCollection services)
    {
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IAppointmentService, AppointmentService>();
        services.AddScoped<IPatientService, PatientService>();
        services.AddScoped<IDoctorService, DoctorService>();
        services.AddScoped<IAdminService, AdminService>();
    }
}