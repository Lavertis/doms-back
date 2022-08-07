using DoctorsOfficeApi.Models.Requests;
using DoctorsOfficeApi.Models.Responses;
using MediatR;

namespace DoctorsOfficeApi.CQRS.Commands.Authenticate;

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