using AutoMapper;
using DoctorsOffice.Domain.DTO.Requests;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Entities;
using DoctorsOffice.Domain.Entities.UserTypes;
using DoctorsOffice.Infrastructure.AutoMapper.Profiles;
using Microsoft.Extensions.DependencyInjection;

namespace DoctorsOffice.Infrastructure.AutoMapper;

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
            options.CreateMap<Timetable, TimetableResponse>();
            options.CreateMap<DrugItem, DrugItemResponse>();
            options.CreateMap<SickLeave, SickLeaveResponse>();
            options.CreateMap<Appointment, AppointmentResponse>();
            options.CreateMap<AppointmentStatus, AppointmentStatusResponse>();
            options.CreateMap<AppointmentType, AppointmentTypeResponse>();
            
            options.AddProfile<AdminResponseMappingProfile>();
            options.AddProfile<DoctorResponseMappingProfile>();
            options.AddProfile<PatientResponseMappingProfile>();
            options.AddProfile<AppointmentSearchResponseMappingProfile>();
            options.AddProfile<PrescriptionResponseMappingProfile>();
        });

        var mapper = mapperConfiguration.CreateMapper();
        return mapper;
    }
}