using AutoMapper;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Repositories;
using DoctorsOffice.Domain.Utils;
using DoctorsOffice.Domain.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DoctorsOffice.Application.CQRS.Queries.Patients.GetPatientsFiltered;

public class GetPatientsFilteredHandler
    : IRequestHandler<GetPatientsFilteredQuery, HttpResult<PagedResponse<PatientResponse>>>
{
    private readonly IMapper _mapper;
    private readonly IPatientRepository _patientRepository;

    public GetPatientsFilteredHandler(IPatientRepository patientRepository, IMapper mapper)
    {
        _patientRepository = patientRepository;
        _mapper = mapper;
    }

    public Task<HttpResult<PagedResponse<PatientResponse>>> Handle(GetPatientsFilteredQuery request,
        CancellationToken cancellationToken)
    {
        var patientsQueryable = _patientRepository.GetAll()
            .Include(p => p.AppUser)
            .AsQueryable();

        var queryParams = request.QueryParams;
        if (queryParams?.FirstName != null)
        {
            patientsQueryable = patientsQueryable.Where(p =>
                p.AppUser.FirstName.ToUpper().Contains(queryParams.FirstName.ToUpper())
            );
        }

        if (queryParams?.LastName != null)
        {
            patientsQueryable = patientsQueryable.Where(p =>
                p.AppUser.LastName.ToUpper().Contains(queryParams.LastName.ToUpper())
            );
        }

        if (queryParams?.Email != null)
        {
            patientsQueryable = patientsQueryable.Where(p =>
                p.AppUser.Email.ToUpper().Contains(queryParams.Email.ToUpper())
            );
        }

        if (queryParams?.NationalId != null)
        {
            patientsQueryable = patientsQueryable.Where(p =>
                p.NationalId.ToUpper().Contains(queryParams.NationalId.ToUpper())
            );
        }

        if (queryParams?.PhoneNumber != null)
        {
            patientsQueryable = patientsQueryable.Where(p =>
                p.AppUser.PhoneNumber.ToUpper().Contains(queryParams.PhoneNumber.ToUpper())
            );
        }

        if (queryParams?.AccountCreationDateBegin != null && queryParams.AccountCreationDateEnd != null)
        {
            patientsQueryable = patientsQueryable.Where(p =>
                p.CreatedAt >= queryParams.AccountCreationDateBegin &&
                p.CreatedAt <= queryParams.AccountCreationDateEnd
            );
        }

        var patientResponses = patientsQueryable
            .Select(patient => _mapper.Map<PatientResponse>(patient));

        return Task.FromResult(PaginationUtils.CreatePagedHttpResult(patientResponses, request.PaginationFilter));
    }
}