using DoctorsOfficeApi.Models.Requests;
using DoctorsOfficeApi.Models.Responses;
using MediatR;

namespace DoctorsOfficeApi.CQRS.Commands.RefreshToken;

public class RefreshTokenCommand : IRequest<AuthenticateResponse>
{
    public readonly string? IpAddress;
    public readonly string RefreshToken;

    public RefreshTokenCommand(RefreshTokenRequest request, string? ipAddress)
    {
        RefreshToken = request.RefreshToken;
        IpAddress = ipAddress;
    }
}