using DoctorsOffice.Domain.DTO.Requests;
using DoctorsOffice.Infrastructure.Identity;
using FluentValidation;

namespace DoctorsOffice.Application.CQRS.Commands.Doctors.CreateDoctor;

public class CreateDoctorRequestValidator : AbstractValidator<CreateDoctorRequest>
{
    public CreateDoctorRequestValidator(AppUserManager appUserManager)
    {
        CascadeMode = CascadeMode.Stop;

        RuleFor(e => e.UserName)
            .NotEmpty()
            .WithMessage("Username is required")
            .MinimumLength(4)
            .WithMessage("Username must be at least 4 characters long")
            .MaximumLength(16)
            .WithMessage("Username must be at most 16 characters long")
            .MustAsync(async (userName, _) => !await appUserManager.ExistsByUserNameAsync(userName))
            .WithMessage("Username already exists");

        RuleFor(e => e.Email)
            .NotEmpty()
            .WithMessage("Email is required")
            .EmailAddress()
            .WithMessage("Email must be a valid email address")
            .MustAsync(async (email, _) => !await appUserManager.ExistsByEmailAsync(email))
            .WithMessage("Email already exists");

        RuleFor(e => e.PhoneNumber)
            .NotEmpty()
            .WithMessage("Phone number is required");
    }
}