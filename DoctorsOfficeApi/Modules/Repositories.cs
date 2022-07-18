using DoctorsOfficeApi.Repositories.AdminRepository;
using DoctorsOfficeApi.Repositories.AppointmentRepository;
using DoctorsOfficeApi.Repositories.AppointmentStatusRepository;
using DoctorsOfficeApi.Repositories.AppointmentTypeRepository;
using DoctorsOfficeApi.Repositories.DoctorRepository;
using DoctorsOfficeApi.Repositories.PatientRepository;

namespace DoctorsOfficeApi.Modules;

public static class Repositories
{
    public static void AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IAdminRepository, AdminRepository>();
        services.AddScoped<IDoctorRepository, DoctorRepository>();
        services.AddScoped<IPatientRepository, PatientRepository>();
        services.AddScoped<IAppointmentRepository, AppointmentRepository>();
        services.AddScoped<IAppointmentStatusRepository, AppointmentStatusRepository>();
        services.AddScoped<IAppointmentTypeRepository, AppointmentTypeRepository>();
    }
}