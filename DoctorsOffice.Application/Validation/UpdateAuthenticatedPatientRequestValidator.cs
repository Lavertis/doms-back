using System.Security.Claims;
using DoctorsOffice.Domain.DTO.Requests;
using DoctorsOffice.Infrastructure.Identity;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace DoctorsOffice.Application.Validation;

public class UpdateAuthenticatedPatientRequestValidator : AbstractValidator<UpdateAuthenticatedPatientRequest>
{
    public UpdateAuthenticatedPatientRequestValidator(
        IHttpContextAccessor httpContextAccessor,
        AppUserManager appUserManager)
    {
        CascadeMode = CascadeMode.Stop;

        var httpContext = httpContextAccessor.HttpContext!;
        var authenticatedUserId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var authenticatedUser = appUserManager.FindByIdAsync(authenticatedUserId).Result;

        When(req => req.UserName is not null, () =>
        {
            RuleFor(e => e.UserName)
                .MinimumLength(4)
                .WithMessage("Username must be at least 4 characters long")
                .MaximumLength(16)
                .WithMessage("Username must be at most 16 characters long")
                .MustAsync(async (userName, _) => (await appUserManager.FindByNameAsync(userName)).IsError)
                .WithMessage("Username already exists");
        });

        RuleFor(e => e.FirstName)
            .MinimumLength(4)
            .WithMessage("First name must be at least 4 characters long")
            .When(e => e.FirstName is not null)
            .MaximumLength(100)
            .WithMessage("First name must be at most 100 characters long")
            .When(e => e.FirstName is not null);

        RuleFor(e => e.LastName)
            .MinimumLength(4)
            .WithMessage("Last name must be at least 4 characters long")
            .When(e => e.LastName is not null)
            .MaximumLength(100)
            .WithMessage("First name must be at most 100 characters long")
            .When(e => e.LastName is not null);

        When(req => req.Email is not null, () =>
        {
            RuleFor(e => e.Email)
                .EmailAddress()
                .WithMessage("Email must be a valid email address")
                .MustAsync(async (email, _) => (await appUserManager.FindByEmailAsync(email)).IsError)
                .WithMessage("Email already exists");
        });

        RuleFor(e => e.NationalId)
            .NotEmpty()
            .WithMessage("National ID is required")
            .MaximumLength(50)
            .WithMessage("National ID must be at most 50 characters long")
            .When(e => e.NationalId is not null);

        RuleFor(e => e.PhoneNumber)
            .NotEmpty()
            .WithMessage("Phone number cannot be empty")
            .When(e => e.PhoneNumber is not null);

        RuleFor(e => e.Address)
            .MinimumLength(4)
            .WithMessage("Address must be at least 4 characters long")
            .When(e => e.Address is not null)
            .MaximumLength(100)
            .WithMessage("Address must be at most 100 characters long")
            .When(e => e.Address is not null);

        RuleFor(e => e.DateOfBirth)
            .Must(date => date <= DateTime.UtcNow)
            .WithMessage("Date of birth must be in the past")
            .When(e => e.DateOfBirth.HasValue);

        When(req => req.NewPassword is not null, () =>
        {
            RuleFor(e => e.NewPassword)
                .MinimumLength(8)
                .WithMessage("NewPassword must be at least 8 characters long")
                .MaximumLength(50)
                .WithMessage("NewPassword must be at most 50 characters long");
        });

        RuleFor(e => e.CurrentPassword)
            .NotEmpty()
            .WithMessage("Current password is required")
            .MustAsync(async (currentPassword, _) =>
            {
                var result =
                    await appUserManager.ValidatePasswordAsync(authenticatedUser.UserName, currentPassword);
                return !result.IsError && result.Value;
            })
            .WithMessage("Current password is incorrect");
    }
}