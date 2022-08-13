using DoctorsOfficeApi.Models.Responses;
using MediatR;

namespace DoctorsOffice.Application.CQRS.Queries.GetUserById;

public class GetUserByIdQuery : IRequest<UserResponse>
{
    public readonly Guid UserId;

    public GetUserByIdQuery(Guid userId)
    {
        UserId = userId;
    }
}