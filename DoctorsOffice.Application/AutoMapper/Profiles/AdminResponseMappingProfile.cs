using AutoMapper;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Entities.UserTypes;

namespace DoctorsOffice.Application.AutoMapper.Profiles;

public class AdminResponseMappingProfile : Profile
{
    public AdminResponseMappingProfile()
    {
        CreateMap<Admin, AdminResponse>()
            .ForMember(adminResponse => adminResponse.UserName, options =>
                options.MapFrom(admin => admin.AppUser.UserName)
            )
            .ForMember(adminResponse => adminResponse.Email, options =>
                options.MapFrom(admin => admin.AppUser.Email)
            );
    }
}