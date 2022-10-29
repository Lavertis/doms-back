using DoctorsOffice.Domain.Utils;
using DoctorsOffice.Infrastructure.Identity;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace DoctorsOffice.Application.CQRS.Commands.Users.ConfirmEmail;

public class ConfirmEmailHandler : IRequestHandler<ConfirmEmailCommand, HttpResult<bool>>
{
    private readonly AppUserManager _appUserManager;

    public ConfirmEmailHandler(AppUserManager appUserManager)
    {
        _appUserManager = appUserManager;
    }

    public async Task<HttpResult<bool>> Handle(ConfirmEmailCommand request, CancellationToken cancellationToken)
    {
        var result = new HttpResult<bool>();
        var data = request.Data;
        var findUserResult = await _appUserManager.FindByEmailAsync(data.Email);
        if (findUserResult.IsError || findUserResult.Value == null)
        {
            return result
                .WithError(findUserResult.Error)
                .WithStatusCode(StatusCodes.Status404NotFound);
        }

        var user = findUserResult.Value;
        var confirmEmailResult = await _appUserManager.ConfirmEmailAsync(user, data.Token);
        if (!confirmEmailResult.Succeeded)
        {
            return result
                .WithError(new Error {Message = confirmEmailResult.Errors.Select(x => x.Description).First()})
                .WithStatusCode(StatusCodes.Status400BadRequest);
        }

        return result.WithValue(true);
    }
}