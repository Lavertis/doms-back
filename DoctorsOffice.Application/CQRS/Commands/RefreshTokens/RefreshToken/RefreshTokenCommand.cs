using DoctorsOffice.Domain.DTO.Requests;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Utils;
using MediatR;

namespace DoctorsOffice.Application.CQRS.Commands.RefreshTokens.RefreshToken;

public class RefreshTokenCommand : IRequest<HttpResult<AuthenticateResponse>>
{
    public readonly string? IpAddress;
    public readonly string RefreshToken;

    public RefreshTokenCommand(RefreshTokenRequest request, string? ipAddress)
    {
        RefreshToken = request.RefreshToken;
        IpAddress = ipAddress;
    }
}