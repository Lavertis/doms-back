using DoctorsOfficeApi.Models.Responses;
using MediatR;

namespace DoctorsOfficeApi.CQRS.Commands.RefreshToken;

public class RefreshTokenCommand : IRequest<AuthenticateResponse>
{
    public string RefreshToken { get; set; } = default!;
    public string? IpAddress { get; set; }

    public RefreshTokenCommand()
    {
    }

    public RefreshTokenCommand(string refreshToken, string? ipAddress)
    {
        RefreshToken = refreshToken;
        IpAddress = ipAddress;
    }
}