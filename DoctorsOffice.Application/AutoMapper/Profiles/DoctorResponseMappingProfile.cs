using AutoMapper;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Entities.UserTypes;

namespace DoctorsOffice.Application.AutoMapper.Profiles;

public class DoctorResponseMappingProfile : Profile
{
    public DoctorResponseMappingProfile()
    {
        CreateMap<Doctor, DoctorResponse>()
            .ForMember(d => d.UserName, opt =>
                opt.MapFrom(doctor => doctor.AppUser.UserName))
            .ForMember(d => d.Email, opt =>
                opt.MapFrom(doctor => doctor.AppUser.Email))
            .ForMember(d => d.PhoneNumber, opt =>
                opt.MapFrom(doctor => doctor.AppUser.PhoneNumber))
            .ForMember(d => d.FirstName, opt =>
                opt.MapFrom(doctor => doctor.AppUser.FirstName))
            .ForMember(d => d.LastName, opt =>
                opt.MapFrom(doctor => doctor.AppUser.LastName));
    }
}