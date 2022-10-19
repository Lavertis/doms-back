using AutoMapper;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Entities.UserTypes;

namespace DoctorsOffice.Application.AutoMapper.Profiles;

public class PatientResponseMappingProfile : Profile
{
    public PatientResponseMappingProfile()
    {
        CreateMap<Patient, PatientResponse>()
            .ForMember(d => d.UserName, opt =>
                opt.MapFrom(patient => patient.AppUser.UserName))
            .ForMember(d => d.Email, opt =>
                opt.MapFrom(patient => patient.AppUser.Email))
            .ForMember(d => d.PhoneNumber, opt =>
                opt.MapFrom(patient => patient.AppUser.PhoneNumber))
            .ForMember(d => d.FirstName, opt =>
                opt.MapFrom(doctor => doctor.AppUser.FirstName))
            .ForMember(d => d.LastName, opt =>
                opt.MapFrom(doctor => doctor.AppUser.LastName));
    }
}