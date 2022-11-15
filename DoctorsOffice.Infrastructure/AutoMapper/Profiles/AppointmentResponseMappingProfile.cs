using AutoMapper;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Entities;

namespace DoctorsOffice.Infrastructure.AutoMapper.Profiles;

public class AppointmentResponseMappingProfile : Profile
{
    public AppointmentResponseMappingProfile()
    {
        CreateMap<Appointment, AppointmentResponse>()
            .ForMember(d => d.Status, opt =>
                opt.MapFrom(appointment => appointment.Status.Name))
            .ForMember(d => d.Type, opt =>
                opt.MapFrom(appointment => appointment.Type.Name));
    }
}