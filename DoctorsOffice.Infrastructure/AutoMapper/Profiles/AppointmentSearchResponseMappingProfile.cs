using AutoMapper;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Entities;

namespace DoctorsOffice.Infrastructure.AutoMapper.Profiles;

public class AppointmentSearchResponseMappingProfile : Profile
{
    public AppointmentSearchResponseMappingProfile()
    {
        CreateMap<Appointment, AppointmentSearchResponse>()
            .ForMember(d => d.PatientFirstName, opt =>
                opt.MapFrom(appointment => appointment.Patient.AppUser.FirstName))
            .ForMember(d => d.PatientLastName, opt =>
                opt.MapFrom(appointment => appointment.Patient.AppUser.LastName))
            .ForMember(d => d.PatientEmail, opt =>
                opt.MapFrom(appointment => appointment.Patient.AppUser.Email))
            .ForMember(d => d.PatientPhoneNumber, opt =>
                opt.MapFrom(appointment => appointment.Patient.AppUser.PhoneNumber))
            .ForMember(appointment => appointment.DoctorFirstName, opt =>
                opt.MapFrom(appointment => appointment.Doctor.AppUser.FirstName))
            .ForMember(appointment => appointment.DoctorLastName, opt =>
                opt.MapFrom(appointment => appointment.Doctor.AppUser.LastName));
    }
}