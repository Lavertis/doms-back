using DoctorsOfficeApi.Entities;
using MediatR;

namespace DoctorsOfficeApi.CQRS.Queries.GetRefreshTokensByUserId;

public class GetRefreshTokensByUserIdQuery : IRequest<IList<RefreshToken>>
{
    public string UserId { get; set; }

    public GetRefreshTokensByUserIdQuery(string userId)
    {
        UserId = userId;
    }
}