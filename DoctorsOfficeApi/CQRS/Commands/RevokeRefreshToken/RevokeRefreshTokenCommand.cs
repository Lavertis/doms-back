using MediatR;

namespace DoctorsOfficeApi.CQRS.Commands.RevokeRefreshToken;

public class RevokeRefreshTokenCommand : IRequest<Unit>
{
    public string RefreshToken { get; set; } = default!;
    public string? IpAddress { get; set; } = default!;

    public RevokeRefreshTokenCommand()
    {
    }

    public RevokeRefreshTokenCommand(string refreshToken, string? ipAddress)
    {
        RefreshToken = refreshToken;
        IpAddress = ipAddress;
    }
}