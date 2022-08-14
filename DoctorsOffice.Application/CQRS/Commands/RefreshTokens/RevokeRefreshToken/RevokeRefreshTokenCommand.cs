using DoctorsOffice.Domain.DTO.Requests;
using DoctorsOffice.Domain.Utils;
using MediatR;

namespace DoctorsOffice.Application.CQRS.Commands.RefreshTokens.RevokeRefreshToken;

public class RevokeRefreshTokenCommand : IRequest<HttpResult<Unit>>
{
    public readonly string? IpAddress;
    public readonly string RefreshToken;

    public RevokeRefreshTokenCommand(RevokeRefreshTokenRequest request, string? ipAddress)
    {
        RefreshToken = request.RefreshToken;
        IpAddress = ipAddress;
    }
}