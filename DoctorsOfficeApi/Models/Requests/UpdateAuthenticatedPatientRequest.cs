using System.Security.Claims;
using DoctorsOfficeApi.Entities.UserTypes;
using DoctorsOfficeApi.Exceptions;
using DoctorsOfficeApi.Services.UserService;
using FluentValidation;
using Microsoft.AspNetCore.Identity;

namespace DoctorsOfficeApi.Models.Requests;

public class UpdateAuthenticatedPatientRequest
{
    public string? UserName { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? NewPassword { get; set; }
    public string CurrentPassword { get; set; } = default!;
}

public class UpdateAuthenticatedPatientRequestValidator : AbstractValidator<UpdateAuthenticatedPatientRequest>
{
    [Obsolete("Obsolete")] // TODO replace OnFailure custom validator
    public UpdateAuthenticatedPatientRequestValidator(
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
                .Must(email =>
                {
                    var conflictUser = userManager.FindByEmailAsync(email).Result;
                    return conflictUser is null || conflictUser.Id.ToString() == authenticatedUserId;
                })
                .OnFailure(request => throw new ConflictException("Email already exists"))
                .WithMessage("Email is already taken");
        });

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
                .WithMessage("Password must be at least 8 characters long")
                .MaximumLength(50)
                .WithMessage("Password must be at most 50 characters long");
        });

        RuleFor(e => e.CurrentPassword)
            .NotEmpty()
            .WithMessage("Current password is required")
            .Must(currentPassword => userService.ValidateUserPasswordAsync(authenticatedUser.UserName, currentPassword).Result)
            .WithMessage("Current password is incorrect");
    }
}