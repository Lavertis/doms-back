using DoctorsOffice.Domain.DTO.Requests;
using DoctorsOffice.Domain.DTO.Responses;
using MediatR;

namespace DoctorsOffice.Application.CQRS.Commands.Authenticate;

public class AuthenticateCommand : IRequest<AuthenticateResponse>
{
    public readonly string? IpAddress;
    public readonly string UserName;

    public AuthenticateCommand(AuthenticateRequest request, string? ipAddress)
    {
        UserName = request.UserName;
        IpAddress = ipAddress;
    }
}