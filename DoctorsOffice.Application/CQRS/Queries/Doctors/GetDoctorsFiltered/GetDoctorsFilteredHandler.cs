using AutoMapper;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Repositories;
using DoctorsOffice.Domain.Utils;
using DoctorsOffice.Domain.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DoctorsOffice.Application.CQRS.Queries.Doctors.GetDoctorsFiltered;

public class GetDoctorsFilteredHandler
    : IRequestHandler<GetDoctorsFilteredQuery, HttpResult<PagedResponse<DoctorResponse>>>
{
    private readonly IDoctorRepository _doctorRepository;
    private readonly IMapper _mapper;

    public GetDoctorsFilteredHandler(IDoctorRepository doctorRepository, IMapper mapper)
    {
        _doctorRepository = doctorRepository;
        _mapper = mapper;
    }

    public Task<HttpResult<PagedResponse<DoctorResponse>>> Handle(GetDoctorsFilteredQuery request,
        CancellationToken cancellationToken)
    {
        var doctorsQueryable = _doctorRepository.GetAll()
            .Include(d => d.AppUser)
            .AsQueryable();

        var queryParams = request.QueryParams;
        if (queryParams?.FirstName != null)
        {
            doctorsQueryable = doctorsQueryable.Where(d =>
                d.AppUser.FirstName.ToUpper().Contains(queryParams.FirstName.ToUpper())
            );
        }

        if (queryParams?.LastName != null)
        {
            doctorsQueryable = doctorsQueryable.Where(d =>
                d.AppUser.LastName.ToUpper().Contains(queryParams.LastName.ToUpper())
            );
        }

        if (queryParams?.Email != null)
        {
            doctorsQueryable = doctorsQueryable.Where(d =>
                d.AppUser.Email.ToUpper().Contains(queryParams.Email.ToUpper())
            );
        }

        if (queryParams?.PhoneNumber != null)
        {
            doctorsQueryable = doctorsQueryable.Where(d =>
                d.AppUser.PhoneNumber.ToUpper().Contains(queryParams.PhoneNumber.ToUpper())
            );
        }

        if (queryParams?.AccountCreationDateBegin != null && queryParams.AccountCreationDateEnd != null)
        {
            doctorsQueryable = doctorsQueryable.Where(d =>
                d.CreatedAt >= queryParams.AccountCreationDateBegin &&
                d.CreatedAt <= queryParams.AccountCreationDateEnd
            );
        }

        var doctorResponses = doctorsQueryable
            .Select(doctor => _mapper.Map<DoctorResponse>(doctor));

        return Task.FromResult(PaginationUtils.CreatePagedHttpResult(doctorResponses, request.PaginationFilter));
    }
}