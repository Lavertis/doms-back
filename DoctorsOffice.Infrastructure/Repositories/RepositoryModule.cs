using DoctorsOffice.Domain.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace DoctorsOffice.Infrastructure.Repositories;

public static class RepositoryModule
{
    public static void AddRepositoryModule(this IServiceCollection services)
    {
        services.AddScoped<IAdminRepository, AdminRepository>();
        services.AddScoped<IDoctorRepository, DoctorRepository>();
        services.AddScoped<IPatientRepository, PatientRepository>();
        services.AddScoped<IAppointmentRepository, AppointmentRepository>();
        services.AddScoped<IAppointmentStatusRepository, AppointmentStatusRepository>();
        services.AddScoped<IAppointmentTypeRepository, AppointmentTypeRepository>();
        services.AddScoped<IPrescriptionRepository, PrescriptionRepository>();
        services.AddScoped<IDrugItemRepository, DrugItemRepository>();
    }
}