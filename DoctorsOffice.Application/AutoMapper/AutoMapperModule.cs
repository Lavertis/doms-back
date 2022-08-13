using AutoMapper;
using Microsoft.Extensions.DependencyInjection;

namespace DoctorsOffice.Application.AutoMapper;

public static class AutoMapperModule
{
    public static void AddAutoMapper(this IServiceCollection services)
    {
        var mapperConfiguration = new MapperConfiguration(options =>
        {
            options.CreateMaps();
            options.AddProfiles();
        });

        var mapper = mapperConfiguration.CreateMapper();
        services.AddSingleton(mapper);
    }
}