using DoctorsOfficeApi.Entities;
using MediatR;

namespace DoctorsOfficeApi.CQRS.Queries.GetRefreshTokensByUserId;

public class GetRefreshTokensByUserIdQuery : IRequest<IList<RefreshToken>>
{
    public readonly Guid UserId;

    public GetRefreshTokensByUserIdQuery(Guid userId)
    {
        UserId = userId;
    }
}