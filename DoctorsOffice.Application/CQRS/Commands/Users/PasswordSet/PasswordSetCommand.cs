using DoctorsOffice.Domain.DTO.Requests;
using DoctorsOffice.Domain.Utils;
using MediatR;

namespace DoctorsOffice.Application.CQRS.Commands.Users.PasswordSet;

public class PasswordSetCommand : IRequest<HttpResult<bool>>
{
    public readonly PasswordSetRequest Data;

    public PasswordSetCommand(PasswordSetRequest data)
    {
        Data = data;
    }
}