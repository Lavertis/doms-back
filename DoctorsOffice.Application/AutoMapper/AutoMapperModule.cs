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
        var mapper = CreateAutoMapper();
        services.AddSingleton(mapper);
    }

    public static IMapper CreateAutoMapper()
    {
        var mapperConfiguration = new MapperConfiguration(options =>
        {
            options.CreateMap<CreateUserRequest, AppUser>();
            options.CreateMap<AppUser, UserResponse>();
            options.AddProfile(new AdminResponseMappingProfile());
            options.AddProfile(new DoctorResponseMappingProfile());
            options.AddProfile(new PatientResponseMappingProfile());
            options.AddProfile(new AppointmentResponseMappingProfile());
            options.AddProfile(new AppointmentSearchResponseMappingProfile());
            options.AddProfile(new PrescriptionResponseMappingProfile());
        });

        var mapper = mapperConfiguration.CreateMapper();
        return mapper;
    }
}