using DoctorsOfficeApi.Models.Requests;
using MediatR;

namespace DoctorsOfficeApi.CQRS.Commands.RevokeRefreshToken;

public class RevokeRefreshTokenCommand : IRequest<Unit>
{
    public readonly string? IpAddress;
    public readonly string RefreshToken;

    public RevokeRefreshTokenCommand(RevokeRefreshTokenRequest request, string? ipAddress)
    {
        RefreshToken = request.RefreshToken;
        IpAddress = ipAddress;
    }
}