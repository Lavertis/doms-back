using AutoMapper;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Entities;

namespace DoctorsOffice.Application.AutoMapper.Profiles;

public class AppointmentSearchResponseMappingProfile : Profile
{
    public AppointmentSearchResponseMappingProfile()
    {
        CreateMap<Appointment, AppointmentSearchResponse>()
            .ForMember(d => d.PatientFirstName, opt =>
                opt.MapFrom(appointment => appointment.Patient.FirstName))
            .ForMember(d => d.PatientLastName, opt =>
                opt.MapFrom(appointment => appointment.Patient.LastName))
            .ForMember(d => d.PatientEmail, opt =>
                opt.MapFrom(appointment => appointment.Patient.AppUser.Email))
            .ForMember(d => d.PatientPhoneNumber, opt =>
                opt.MapFrom(appointment => appointment.Patient.AppUser.PhoneNumber))
            .ForMember(d => d.Status, opt =>
                opt.MapFrom(appointment => appointment.Status.Name))
            .ForMember(d => d.Type, opt =>
                opt.MapFrom(appointment => appointment.Type.Name));
    }
}