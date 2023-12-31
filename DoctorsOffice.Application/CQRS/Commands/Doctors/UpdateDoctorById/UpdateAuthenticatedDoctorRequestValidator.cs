﻿using System.Security.Claims;
using DoctorsOffice.Domain.DTO.Requests;
using DoctorsOffice.Infrastructure.Identity;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace DoctorsOffice.Application.CQRS.Commands.Doctors.UpdateDoctorById;

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

        When(req => req.Email is not null, () =>
        {
            RuleFor(e => e.Email)
                .EmailAddress()
                .WithMessage("Email must be a valid email address")
                .MustAsync(async (email, _) => (await appUserManager.FindByEmailAsync(email)).IsError)
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
                return !result.IsError && result.Value;
            })
            .WithMessage("Current password is incorrect");

        RuleFor(e => e.PhoneNumber)
            .NotEmpty()
            .WithMessage("Phone number cannot be empty")
            .When(e => e.PhoneNumber is not null);
    }
}