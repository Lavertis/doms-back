using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Utils;
using MediatR;

namespace DoctorsOffice.Application.CQRS.Queries.Users.GetAllUsers;

public class GetAllUsersQuery : IRequest<HttpResult<IEnumerable<UserResponse>>>
{
}