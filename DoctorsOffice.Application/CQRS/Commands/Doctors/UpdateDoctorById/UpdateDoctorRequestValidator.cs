using DoctorsOffice.Domain.DTO.Requests;
using DoctorsOffice.Infrastructure.Identity;
using FluentValidation;

namespace DoctorsOffice.Application.CQRS.Commands.Doctors.UpdateDoctorById;

public class UpdateDoctorRequestValidator : AbstractValidator<UpdateDoctorRequest>
{
    public UpdateDoctorRequestValidator(AppUserManager appUserManager)
    {
        CascadeMode = CascadeMode.Stop;

        When(req => req.Email is not null, () =>
        {
            RuleFor(e => e.Email)
                .EmailAddress()
                .WithMessage("Email must be a valid email address")
                .MustAsync(async (email, _) => (await appUserManager.FindByEmailAsync(email)).IsError)
                .WithMessage("Email already exists");
        });

        RuleFor(e => e.PhoneNumber)
            .NotEmpty()
            .WithMessage("Phone number cannot be empty")
            .When(e => e.PhoneNumber is not null);

        When(req => req.NewPassword is not null, () =>
        {
            RuleFor(e => e.NewPassword)
                .MinimumLength(8)
                .WithMessage("NewPassword must be at least 8 characters long")
                .MaximumLength(50)
                .WithMessage("NewPassword must be at most 50 characters long");
        });
    }
}