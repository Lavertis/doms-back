using DoctorsOffice.Application.Services.User;
using DoctorsOffice.Domain.DTO.Requests;
using FluentValidation;

namespace DoctorsOffice.Application.Validation;

public class AuthenticateRequestValidator : AbstractValidator<AuthenticateRequest>
{
    public AuthenticateRequestValidator(IUserService userService)
    {
        CascadeMode = CascadeMode.Stop;

        RuleFor(e => e.UserName)
            .MustAsync(async (userName, cancellationToken) =>
                await userService.UserNameExistsAsync(userName, cancellationToken))
            .WithMessage("Username does not exist");

        RuleFor(e => new {e.UserName, e.Password})
            .MustAsync(async (args, _) =>
                await userService.ValidateUserPasswordAsync(args.UserName, args.Password))
            .WithName("Password")
            .WithMessage("Password is incorrect");
    }
}