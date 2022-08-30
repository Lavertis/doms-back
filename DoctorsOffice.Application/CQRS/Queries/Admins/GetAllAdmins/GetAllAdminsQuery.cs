using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Filters;
using DoctorsOffice.Domain.Utils;
using DoctorsOffice.Domain.Wrappers;
using MediatR;

namespace DoctorsOffice.Application.CQRS.Queries.Admins.GetAllAdmins;

public class GetAllAdminsQuery : IRequest<HttpResult<PagedResponse<AdminResponse>>>
{
    public PaginationFilter? PaginationFilter { get; set; }
}