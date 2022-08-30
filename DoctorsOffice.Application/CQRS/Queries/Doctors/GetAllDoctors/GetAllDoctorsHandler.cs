using AutoMapper;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Repositories;
using DoctorsOffice.Domain.Utils;
using DoctorsOffice.Domain.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DoctorsOffice.Application.CQRS.Queries.Doctors.GetAllDoctors;

public class GetAllDoctorsHandler : IRequestHandler<GetAllDoctorsQuery, HttpResult<PagedResponse<DoctorResponse>>>
{
    private readonly IDoctorRepository _doctorRepository;
    private readonly IMapper _mapper;

    public GetAllDoctorsHandler(IDoctorRepository doctorRepository, IMapper mapper)
    {
        _doctorRepository = doctorRepository;
        _mapper = mapper;
    }

    public Task<HttpResult<PagedResponse<DoctorResponse>>> Handle(GetAllDoctorsQuery request,
        CancellationToken cancellationToken)
    {
        var doctorResponsesQueryable = _doctorRepository.GetAll()
            .Include(a => a.AppUser)
            .Select(doctor => _mapper.Map<DoctorResponse>(doctor));

        return Task.FromResult(PaginationUtils.CreatePagedHttpResult(doctorResponsesQueryable, request.PaginationFilter));
    }
}