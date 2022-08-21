using DoctorsOffice.Domain.DTO.Requests;
using DoctorsOffice.Infrastructure.Identity;
using FluentValidation;

namespace DoctorsOffice.Application.Validation;

public class AuthenticateRequestValidator : AbstractValidator<AuthenticateRequest>
{
    public AuthenticateRequestValidator(AppUserManager appUserManager)
    {
        CascadeMode = CascadeMode.Stop;

        RuleFor(e => e.UserName)
            .MustAsync(async (userName, _) =>
                await appUserManager.ExistsByUserNameAsync(userName)
            )
            .WithMessage("Username does not exist");

        RuleFor(e => new {e.UserName, e.Password})
            .MustAsync(async (args, _) =>
            {
                var result = await appUserManager.ValidatePasswordAsync(args.UserName, args.Password);
                return result.IsSuccess && result.Value;
            })
            .WithName("NewPassword")
            .WithMessage("NewPassword is incorrect");
    }
}