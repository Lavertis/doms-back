using System.Security.Claims;
using DoctorsOffice.Domain.DTO.Requests;
using DoctorsOffice.Infrastructure.Identity;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace DoctorsOffice.Application.Validation;

public class UpdateAuthenticatedDoctorRequestValidator : AbstractValidator<UpdateAuthenticatedDoctorRequest>
{
    public UpdateAuthenticatedDoctorRequestValidator(
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
                .MustAsync(async (userName, _) => (await appUserManager.FindByNameAsync(userName)).IsFailed)
                .WithMessage("Username already exists");
        });

        When(req => req.Email is not null, () =>
        {
            RuleFor(e => e.Email)
                .EmailAddress()
                .WithMessage("Email must be a valid email address")
                .MustAsync(async (email, _) => (await appUserManager.FindByEmailAsync(email)).IsFailed)
                .WithMessage("Email already exists");
        });

        When(req => req.NewPassword is not null, () =>
        {
            RuleFor(e => e.NewPassword)
                .MinimumLength(8)
                .WithMessage("NewPassword must be at least 8 characters long")
                .MaximumLength(50)
                .WithMessage("NewPassword must be at most 50 characters long")
                .Equal(e => e.ConfirmPassword)
                .WithMessage("Passwords do not match");
        });

        RuleFor(e => e.CurrentPassword)
            .NotEmpty()
            .WithMessage("Current password is required")
            .MustAsync(async (currentPassword, _) =>
            {
                var result = await appUserManager.ValidatePasswordAsync(authenticatedUser.UserName, currentPassword);
                return result.IsSuccess && result.Value;
            })
            .WithMessage("Current password is incorrect");

        RuleFor(e => e.PhoneNumber)
            .NotEmpty()
            .WithMessage("Phone number cannot be empty")
            .When(e => e.PhoneNumber is not null);
    }
}