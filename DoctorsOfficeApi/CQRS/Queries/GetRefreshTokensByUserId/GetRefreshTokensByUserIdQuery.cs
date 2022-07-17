using DoctorsOfficeApi.Entities;
using MediatR;

namespace DoctorsOfficeApi.CQRS.Queries.GetRefreshTokensByUserId;

public class GetRefreshTokensByUserIdQuery : IRequest<IList<RefreshToken>>
{
    public Guid UserId { get; set; }

    public GetRefreshTokensByUserIdQuery(Guid userId)
    {
        UserId = userId;
    }
}