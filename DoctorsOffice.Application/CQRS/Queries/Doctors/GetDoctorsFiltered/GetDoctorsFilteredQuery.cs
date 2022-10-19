using DoctorsOffice.Domain.DTO.QueryParams;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Filters;
using DoctorsOffice.Domain.Utils;
using DoctorsOffice.Domain.Wrappers;
using MediatR;

namespace DoctorsOffice.Application.CQRS.Queries.Doctors.GetDoctorsFiltered;

public class GetDoctorsFilteredQuery : IRequest<HttpResult<PagedResponse<DoctorResponse>>>
{
    public PaginationFilter? PaginationFilter { get; set; }
    public DoctorQueryParams? QueryParams { get; set; }
}