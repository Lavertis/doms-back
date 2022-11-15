using AutoMapper;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Entities;

namespace DoctorsOffice.Infrastructure.AutoMapper.Profiles;

public class PrescriptionResponseMappingProfile : Profile
{
    public PrescriptionResponseMappingProfile()
    {
        CreateMap<Prescription, PrescriptionResponse>();
    }
}