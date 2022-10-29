using DoctorsOffice.Domain.DTO.Requests;
using DoctorsOffice.Domain.Utils;
using MediatR;

namespace DoctorsOffice.Application.CQRS.Commands.Users.ConfirmEmail;

public class ConfirmEmailCommand : IRequest<HttpResult<bool>>
{
    public readonly ConfirmEmailRequest Data;

    public ConfirmEmailCommand(ConfirmEmailRequest data)
    {
        Data = data;
    }
}