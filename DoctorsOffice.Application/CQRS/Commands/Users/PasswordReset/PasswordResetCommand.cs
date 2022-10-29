using DoctorsOffice.Domain.DTO.Requests;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Utils;
using MediatR;

namespace DoctorsOffice.Application.CQRS.Commands.Users.PasswordReset;

public class PasswordResetCommand : IRequest<HttpResult<PasswordResetResponse>>
{
    public readonly PasswordResetRequest Data;

    public PasswordResetCommand(PasswordResetRequest data)
    {
        Data = data;
    }
}