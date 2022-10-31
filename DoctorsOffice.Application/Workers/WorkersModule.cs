using Microsoft.Extensions.DependencyInjection;

namespace DoctorsOffice.Application.Workers;

public static class WorkersModule
{
    public static void AddWorkers(this IServiceCollection services)
    {
        services.AddHostedService<AppointmentReminderWorkerService>();
    }
}