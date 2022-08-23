using AutoMapper;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Entities;

namespace DoctorsOffice.Application.AutoMapper.Profiles;

public class PrescriptionResponseMappingProfile : Profile
{
    public PrescriptionResponseMappingProfile()
    {
        CreateMap<Prescription, PrescriptionResponse>()
            .ForMember(d => d.DrugsIds, opt =>
                opt.MapFrom(prescription => prescription.DrugItems.Select(drugItem => drugItem.Id)));
    }
}