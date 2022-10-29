using DoctorsOffice.Domain.DTO.Requests;
using FluentValidation;

namespace DoctorsOffice.Application.CQRS.Commands.Users.PasswordSet;

public class PasswordSetValidator : AbstractValidator<PasswordSetRequest>
{
    public PasswordSetValidator()
    {
        RuleFor(e => e.NewPassword)
            .NotEmpty()
            .WithMessage("NewPassword is required")
            .MinimumLength(8)
            .WithMessage("NewPassword must be at least 8 characters long")
            .MaximumLength(50)
            .WithMessage("NewPassword must be at most 50 characters long");
    }
}