using DoctorsOffice.Domain.DTO.Requests;
using DoctorsOffice.Domain.DTO.Responses;
using MediatR;

namespace DoctorsOffice.Application.CQRS.Commands.RefreshToken;

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