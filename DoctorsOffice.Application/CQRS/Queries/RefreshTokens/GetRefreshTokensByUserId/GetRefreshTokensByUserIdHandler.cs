using DoctorsOffice.Domain.Entities;
using DoctorsOffice.Domain.Entities.UserTypes;
using DoctorsOffice.Domain.Utils;
using DoctorsOffice.Infrastructure.Identity;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace DoctorsOffice.Application.CQRS.Queries.RefreshTokens.GetRefreshTokensByUserId;

public class GetRefreshTokensByUserIdHandler
    : IRequestHandler<GetRefreshTokensByUserIdQuery, HttpResult<IEnumerable<RefreshToken>>>
{
    private readonly AppUserManager _appUserManager;

    public GetRefreshTokensByUserIdHandler(AppUserManager appUserManager)
    {
        _appUserManager = appUserManager;
    }

    public async Task<HttpResult<IEnumerable<RefreshToken>>> Handle(
        GetRefreshTokensByUserIdQuery request, CancellationToken cancellationToken)
    {
        var result = new HttpResult<IEnumerable<RefreshToken>>();
        var user = await _appUserManager.Users
            .Include(user => user.RefreshTokens)
            .FirstOrDefaultAsync(user => user.Id == request.UserId, cancellationToken: cancellationToken);

        if (user is null)
        {
            return result
                .WithError(new Error {Message = $"{nameof(AppUser)} with id {request.UserId} does not exist"})
                .WithStatusCode(StatusCodes.Status404NotFound);
        }

        return result.WithValue(user.RefreshTokens);
    }
}