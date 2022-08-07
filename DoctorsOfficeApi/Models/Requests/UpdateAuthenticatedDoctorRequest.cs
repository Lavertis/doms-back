using System.Security.Claims;
using DoctorsOfficeApi.Entities.UserTypes;
using DoctorsOfficeApi.Exceptions;
using DoctorsOfficeApi.Services.UserService;
using FluentValidation;
using Microsoft.AspNetCore.Identity;

namespace DoctorsOfficeApi.Models.Requests;

public class UpdateAuthenticatedDoctorRequest
{
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? NewPassword { get; set; }
    public string? ConfirmPassword { get; set; }
    public string CurrentPassword { get; set; } = default!;
}

public class UpdateAuthenticatedDoctorRequestValidator : AbstractValidator<UpdateAuthenticatedDoctorRequest>
{
    [Obsolete("Obsolete")] // TODO replace OnFailure custom validator
    public UpdateAuthenticatedDoctorRequestValidator(
        IHttpContextAccessor httpContextAccessor,
        UserManager<AppUser> userManager,
        IUserService userService)
    {
        var httpContext = httpContextAccessor.HttpContext!;
        var authenticatedUserId = httpContext.User.FindFirstValue(ClaimTypes.Sid)!;
        var authenticatedUser = userManager.FindByIdAsync(authenticatedUserId).Result;

        When(req => req.UserName is not null, () =>
        {
            RuleFor(e => e.UserName)
                .MinimumLength(4)
                .WithMessage("Username must be at least 4 characters long")
                .MaximumLength(16)
                .WithMessage("Username must be at most 16 characters long")
                .Must(email =>
                {
                    var conflictUser = userManager.FindByNameAsync(email).Result;
                    return conflictUser is null || conflictUser.Id.ToString() == authenticatedUserId;
                })
                .OnFailure(request => throw new ConflictException("Username already exists"))
                .WithMessage("Username is already taken");
        });

        When(req => req.Email is not null, () =>
        {
            RuleFor(e => e.Email)
                .EmailAddress()
                .WithMessage("Email must be a valid email address")
                .Must(email =>
                {
                    var conflictUser = userManager.FindByEmailAsync(email).Result;
                    return conflictUser is null || conflictUser.Id.ToString() == authenticatedUserId;
                })
                .OnFailure(request => throw new ConflictException("Email already exists"))
                .WithMessage("Email is already taken");
        });

        When(req => req.NewPassword is not null, () =>
        {
            RuleFor(e => e.NewPassword)
                .MinimumLength(8)
                .WithMessage("Password must be at least 8 characters long")
                .MaximumLength(50)
                .WithMessage("Password must be at most 50 characters long")
                .Equal(e => e.ConfirmPassword)
                .WithMessage("Passwords do not match");
        });

        RuleFor(e => e.CurrentPassword)
            .NotEmpty()
            .WithMessage("Current password is required")
            .Must(currentPassword =>
                userService.ValidateUserPasswordAsync(authenticatedUser.UserName, currentPassword).Result)
            .WithMessage("Current password is incorrect");

        RuleFor(e => e.PhoneNumber)
            .NotEmpty()
            .WithMessage("Phone number cannot be empty")
            .When(e => e.PhoneNumber is not null);
    }
}