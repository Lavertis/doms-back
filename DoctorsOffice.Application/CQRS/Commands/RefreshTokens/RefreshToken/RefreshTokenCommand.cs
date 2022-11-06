using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Utils;
using MediatR;

namespace DoctorsOffice.Application.CQRS.Commands.RefreshTokens.RefreshToken;

public class RefreshTokenCommand : IRequest<HttpResult<AuthenticateResponse>>
{
    public string? RefreshToken { get; set; }
    public string? IpAddress { get; set; }
}