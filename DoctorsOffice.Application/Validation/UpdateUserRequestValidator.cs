using DoctorsOffice.Domain.DTO.Requests;
using DoctorsOffice.Infrastructure.Identity;
using FluentValidation;

namespace DoctorsOffice.Application.Validation;

public class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
{
    public UpdateUserRequestValidator(AppUserManager appUserManager)
    {
        RuleFor(u => u.UserName)
            .MinimumLength(4)
            .WithMessage("Username must be at least 4 characters long")
            .When(u => !string.IsNullOrEmpty(u.UserName))
            .MaximumLength(16)
            .WithMessage("Username must be at most 16 characters long")
            .When(u => !string.IsNullOrEmpty(u.UserName))
            .MustAsync(async (userName, _) =>
                !await appUserManager.ExistsByUserNameAsync(userName))
            .WithMessage("Username already exists")
            .When(u => !string.IsNullOrEmpty(u.UserName));

        RuleFor(u => u.Email)
            .EmailAddress()
            .WithMessage("Email must be a valid email address")
            .When(u => !string.IsNullOrEmpty(u.Email));

        RuleFor(u => u.PhoneNumber);

        RuleFor(u => u.NewPassword)
            .MinimumLength(8)
            .WithMessage("NewPassword must be at least 8 characters long")
            .When(u => !string.IsNullOrEmpty(u.NewPassword))
            .MaximumLength(50)
            .WithMessage("NewPassword must be at most 50 characters long")
            .When(u => !string.IsNullOrEmpty(u.NewPassword))
            .Equal(e => e.ConfirmPassword)
            .WithMessage("Passwords do not match")
            .When(u => !string.IsNullOrEmpty(u.NewPassword));
    }
}