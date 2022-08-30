using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Filters;
using DoctorsOffice.Domain.Utils;
using DoctorsOffice.Domain.Wrappers;
using MediatR;

namespace DoctorsOffice.Application.CQRS.Queries.Users.GetAllUsers;

public class GetAllUsersQuery : IRequest<HttpResult<PagedResponse<UserResponse>>>
{
    public PaginationFilter? PaginationFilter { get; set; }
}