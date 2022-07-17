using DoctorsOfficeApi.Models.Responses;
using MediatR;

namespace DoctorsOfficeApi.CQRS.Queries.GetUserById;

public class GetUserByIdQuery : IRequest<UserResponse>
{
    public Guid UserId { get; set; }

    public GetUserByIdQuery(Guid userId)
    {
        UserId = userId;
    }
}