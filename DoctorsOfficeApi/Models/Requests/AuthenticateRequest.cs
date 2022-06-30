using DoctorsOfficeApi.Services.UserService;
using FluentValidation;

namespace DoctorsOfficeApi.Models.Requests;

public class AuthenticateRequest
{
    public string UserName { get; set; } = default!;
    public string Password { get; set; } = default!;
}

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