using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace DoctorsOffice.Application.CQRS;

public static class MediatorModule
{
    public static void AddMediatorModule(this IServiceCollection services)
    {
        services.AddMediatR(AppDomain.CurrentDomain.GetAssemblies());
    }
}