using DoctorsOffice.Domain.Entities;
using DoctorsOffice.Domain.Utils;
using MediatR;

namespace DoctorsOffice.Application.CQRS.Queries.RefreshTokens.GetRefreshTokensByUserId;

public class GetRefreshTokensByUserIdQuery : IRequest<HttpResult<IEnumerable<RefreshToken>>>
{
    public readonly Guid UserId;

    public GetRefreshTokensByUserIdQuery(Guid userId)
    {
        UserId = userId;
    }
}