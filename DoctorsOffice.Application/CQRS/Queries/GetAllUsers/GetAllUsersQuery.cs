using DoctorsOfficeApi.Models.Responses;
using MediatR;

namespace DoctorsOffice.Application.CQRS.Queries.GetAllUsers;

public class GetAllUsersQuery : IRequest<IList<UserResponse>>
{
}