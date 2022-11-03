using AutoMapper;
using DoctorsOffice.Application.AutoMapper.Profiles;
using DoctorsOffice.Domain.DTO.Requests;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Entities;
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
            options.CreateMap<DrugItem, DrugItemResponse>();
            options.CreateMap<SickLeave, SickLeaveResponse>();
            
            options.AddProfile<AdminResponseMappingProfile>();
            options.AddProfile<DoctorResponseMappingProfile>();
            options.AddProfile<PatientResponseMappingProfile>();
            options.AddProfile<AppointmentResponseMappingProfile>();
            options.AddProfile<AppointmentSearchResponseMappingProfile>();
            options.AddProfile<PrescriptionResponseMappingProfile>();
        });

        var mapper = mapperConfiguration.CreateMapper();
        return mapper;
    }
}