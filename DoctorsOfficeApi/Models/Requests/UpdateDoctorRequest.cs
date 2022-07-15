using System.Security.Claims;
using DoctorsOfficeApi.Entities.UserTypes;
using DoctorsOfficeApi.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Identity;

namespace DoctorsOfficeApi.Models.Requests;

public class UpdateDoctorRequest
{
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? NewPassword { get; set; }
}

public class UpdateDoctorRequestValidator : AbstractValidator<UpdateDoctorRequest>
{
    [Obsolete("")] // TODO replace OnFailure custom validator
    public UpdateDoctorRequestValidator(
        IHttpContextAccessor httpContextAccessor,
        UserManager<AppUser> userManager)
    {
        var httpContext = httpContextAccessor.HttpContext!;
        var authenticatedUserId = httpContext.User.FindFirstValue(ClaimTypes.Sid)!;

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
                    return conflictUser is null || conflictUser.Id == authenticatedUserId;
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
                    return conflictUser is null || conflictUser.Id == authenticatedUserId;
                })
                .OnFailure(request => throw new ConflictException("Email already exists"))
                .WithMessage("Email is already taken");
        });

        RuleFor(e => e.PhoneNumber)
            .NotEmpty()
            .WithMessage("Phone number cannot be empty")
            .When(e => e.PhoneNumber is not null);

        When(req => req.NewPassword is not null, () =>
        {
            RuleFor(e => e.NewPassword)
                .MinimumLength(8)
                .WithMessage("Password must be at least 8 characters long")
                .MaximumLength(50)
                .WithMessage("Password must be at most 50 characters long");
        });
    }
}