using DoctorsOffice.Domain.Entities;
using MediatR;

namespace DoctorsOffice.Application.CQRS.Queries.RefreshTokens.GetRefreshTokensByUserId;

public class GetRefreshTokensByUserIdQuery : IRequest<IList<RefreshToken>>
{
    public readonly Guid UserId;

    public GetRefreshTokensByUserIdQuery(Guid userId)
    {
        UserId = userId;
    }
}