using DoctorsOffice.Domain.Utils;
using DoctorsOffice.Infrastructure.Identity;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace DoctorsOffice.Application.CQRS.Commands.Users.PasswordSet;

public class PasswordSetHandler : IRequestHandler<PasswordSetCommand, HttpResult<bool>>
{
    private readonly AppUserManager _appUserManager;

    public PasswordSetHandler(AppUserManager appUserManager)
    {
        _appUserManager = appUserManager;
    }

    public async Task<HttpResult<bool>> Handle(PasswordSetCommand request, CancellationToken cancellationToken)
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

        var appUser = findUserResult.Value;
        var updatePasswordResult =
            await _appUserManager.ResetPasswordAsync(appUser, data.PasswordResetToken, data.NewPassword);
        if (!updatePasswordResult.Succeeded)
        {
            return result
                .WithStatusCode(StatusCodes.Status400BadRequest)
                .WithError(new Error {Message = updatePasswordResult.Errors.First().Description});
        }

        return result.WithValue(true);
    }
}