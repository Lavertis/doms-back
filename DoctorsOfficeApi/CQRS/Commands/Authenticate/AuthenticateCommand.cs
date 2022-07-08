using DoctorsOfficeApi.Models.Responses;
using MediatR;

namespace DoctorsOfficeApi.CQRS.Commands.Authenticate;

public class AuthenticateCommand : IRequest<AuthenticateResponse>
{
    public string UserName { get; set; }
    public string Password { get; set; }
    public string? IpAddress { get; set; }

    public AuthenticateCommand(string userName, string password, string? ipAddress)
    {
        UserName = userName;
        Password = password;
        IpAddress = ipAddress;
    }
}