using AutoMapper;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Entities.UserTypes;

namespace DoctorsOffice.Infrastructure.AutoMapper.Profiles;

public class AdminResponseMappingProfile : Profile
{
    public AdminResponseMappingProfile()
    {
        CreateMap<Admin, AdminResponse>()
            .ForMember(d => d.UserName, opt =>
                opt.MapFrom(admin => admin.AppUser.UserName))
            .ForMember(d => d.Email, opt =>
                opt.MapFrom(admin => admin.AppUser.Email));
    }
}