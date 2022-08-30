using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Filters;
using DoctorsOffice.Domain.Utils;
using DoctorsOffice.Domain.Wrappers;
using MediatR;

namespace DoctorsOffice.Application.CQRS.Queries.Doctors.GetAllDoctors;

public class GetAllDoctorsQuery : IRequest<HttpResult<PagedResponse<DoctorResponse>>>
{
    public PaginationFilter? PaginationFilter { get; set; }
}