using DoctorsOfficeApi.Models.Responses;
using MediatR;

namespace DoctorsOfficeApi.CQRS.Queries.GetUserById;

public class GetUserByIdQuery : IRequest<UserResponse>
{
    public string UserId { get; set; }

    public GetUserByIdQuery(string userId)
    {
        UserId = userId;
    }
}