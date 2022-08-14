using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Utils;
using MediatR;

namespace DoctorsOffice.Application.CQRS.Queries.Users.GetUserById;

public class GetUserByIdQuery : IRequest<HttpResult<UserResponse>>
{
    public readonly Guid UserId;

    public GetUserByIdQuery(Guid userId)
    {
        UserId = userId;
    }
}