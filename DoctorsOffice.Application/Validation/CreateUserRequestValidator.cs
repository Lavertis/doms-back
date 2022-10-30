using DoctorsOffice.Domain.DTO.Requests;
using DoctorsOffice.Infrastructure.Identity;
using FluentValidation;

namespace DoctorsOffice.Application.Validation;

public class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator(AppUserManager appUserManager, AppRoleManager appRoleManager)
    {
        CascadeMode = CascadeMode.Stop;

        RuleFor(e => e.Email)
            .NotEmpty()
            .WithMessage("Email is required")
            .EmailAddress()
            .WithMessage("Email must be a valid email address");

        RuleFor(e => e.Password)
            .NotEmpty()
            .WithMessage("NewPassword is required")
            .MinimumLength(8)
            .WithMessage("NewPassword must be at least 8 characters long")
            .MaximumLength(50)
            .WithMessage("NewPassword must be at most 50 characters long");

        RuleFor(e => e.PhoneNumber)
            .NotEmpty()
            .WithMessage("Phone number is required");

        RuleFor(e => e.RoleName)
            .NotEmpty()
            .WithMessage("Role name is required")
            .MustAsync(async (roleName, _) => await appRoleManager.RoleExistsAsync(roleName))
            .WithMessage("Role does not exist");
    }
}