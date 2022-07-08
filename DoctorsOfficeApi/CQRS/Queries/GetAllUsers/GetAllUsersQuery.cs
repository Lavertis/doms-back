using DoctorsOfficeApi.Models.Responses;
using MediatR;

namespace DoctorsOfficeApi.CQRS.Queries.GetAllUsers;

public class GetAllUsersQuery : IRequest<IList<UserResponse>>
{
}