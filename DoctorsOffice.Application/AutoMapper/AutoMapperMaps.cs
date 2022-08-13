using AutoMapper;
using DoctorsOffice.Domain.DTO.Requests;
using DoctorsOffice.Domain.Entities.UserTypes;

namespace DoctorsOffice.Application.AutoMapper;

public static class AutoMapperMaps
{
    public static void CreateMaps(this IMapperConfigurationExpression options)
    {
        options.CreateMap<CreateUserRequest, AppUser>();
    }
}