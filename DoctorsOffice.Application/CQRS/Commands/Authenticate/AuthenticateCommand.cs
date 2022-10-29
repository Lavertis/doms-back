using DoctorsOffice.Domain.DTO.Requests;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Utils;
using MediatR;

namespace DoctorsOffice.Application.CQRS.Commands.Authenticate;

public class AuthenticateCommand : IRequest<HttpResult<AuthenticateResponse>>
{
    public readonly string? IpAddress;
    public readonly string Login;
    public readonly string Password;

    public AuthenticateCommand(AuthenticateRequest request, string? ipAddress)
    {
        Login = request.Login;
        Password = request.Password;
        IpAddress = ipAddress;
    }
}