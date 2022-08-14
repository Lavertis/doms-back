using AutoMapper;
using DoctorsOffice.Application.AutoMapper.Profiles;
using DoctorsOffice.Domain.DTO.Requests;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Entities.UserTypes;
using Microsoft.Extensions.DependencyInjection;

namespace DoctorsOffice.Application.AutoMapper;

public static class AutoMapperModule
{
    public static void AddAutoMapperModule(this IServiceCollection services)
    {
        var mapperConfiguration = new MapperConfiguration(options =>
        {
            options.CreateMap<CreateUserRequest, AppUser>();
            options.CreateMap<AppUser, UserResponse>();
            options.AddProfile(new AdminResponseMappingProfile());
        });

        var mapper = mapperConfiguration.CreateMapper();
        services.AddSingleton(mapper);
    }
}