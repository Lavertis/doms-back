using DoctorsOffice.Domain.DTO.Requests;
using DoctorsOffice.Infrastructure.Identity;
using FluentValidation;

namespace DoctorsOffice.Application.CQRS.Commands.Patients.CreatePatient;

public class CreatePatientRequestValidator : AbstractValidator<CreatePatientRequest>
{
    public CreatePatientRequestValidator(AppUserManager appUserManager)
    {
        CascadeMode = CascadeMode.Stop;

        RuleFor(e => e.FirstName)
            .NotEmpty()
            .WithMessage("First name is required")
            .MaximumLength(100)
            .WithMessage("First name must be at most 100 characters long");

        RuleFor(e => e.LastName)
            .NotEmpty()
            .WithMessage("First name is required")
            .MaximumLength(100)
            .WithMessage("First name must be at most 100 characters long");

        RuleFor(e => e.NationalId)
            .NotEmpty()
            .WithMessage("National ID is required")
            .MaximumLength(50)
            .WithMessage("National ID must be at most 50 characters long");

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

        RuleFor(e => e.Address)
            .NotEmpty()
            .WithMessage("Address is required")
            .MaximumLength(100)
            .WithMessage("Address must be at most 100 characters long");

        RuleFor(e => e.DateOfBirth)
            .NotEmpty()
            .WithMessage("Date of birth is required")
            .Must(date => date.Date <= DateTime.UtcNow.Date)
            .WithMessage("Date of birth must be in the past");

        RuleFor(e => e.Password)
            .NotEmpty()
            .WithMessage("NewPassword is required")
            .MinimumLength(8)
            .WithMessage("NewPassword must be at least 8 characters long")
            .MaximumLength(50)
            .WithMessage("NewPassword must be at most 50 characters long")
            .Equal(e => e.ConfirmPassword)
            .WithMessage("Passwords do not match");
    }
}