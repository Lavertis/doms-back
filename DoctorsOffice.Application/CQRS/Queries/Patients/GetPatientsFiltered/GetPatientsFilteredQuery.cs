using DoctorsOffice.Domain.DTO.QueryParams;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Filters;
using DoctorsOffice.Domain.Utils;
using DoctorsOffice.Domain.Wrappers;
using MediatR;

namespace DoctorsOffice.Application.CQRS.Queries.Patients.GetPatientsFiltered;

public class GetPatientsFilteredQuery : IRequest<HttpResult<PagedResponse<PatientResponse>>>
{
    public PaginationFilter? PaginationFilter { get; set; }
    public PatientQueryParams? QueryParams { get; set; }
}