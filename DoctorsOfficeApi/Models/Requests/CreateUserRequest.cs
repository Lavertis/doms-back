using DoctorsOfficeApi.Entities;
using DoctorsOfficeApi.Services.UserService;
using FluentValidation;
using Microsoft.AspNetCore.Identity;

namespace DoctorsOfficeApi.Models.Requests;

public class CreateUserRequest
{
    public string UserName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string Password { get; set; } = default!;
    public string ConfirmPassword { get; set; } = default!;
    public string PhoneNumber { get; set; } = default!;
    public string RoleName { get; set; } = default!;
}

public class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator(IUserService userService, RoleManager<AppRole> roleManager)
    {
        CascadeMode = CascadeMode.Stop;
        RuleFor(e => e.UserName)
            .NotEmpty()
            .WithMessage("Username is required")
            .MinimumLength(4)
            .WithMessage("Username must be at least 4 characters long")
            .MaximumLength(16)
            .WithMessage("Username must be at most 16 characters long")
            .MustAsync(async (userName, cancellationToken) =>
                !await userService.UserNameExistsAsync(userName, cancellationToken))
            .WithMessage("Username already exists");

        RuleFor(e => e.Email)
            .NotEmpty()
            .WithMessage("Email is required")
            .EmailAddress()
            .WithMessage("Email must be a valid email address");

        RuleFor(e => e.Password)
            .NotEmpty()
            .WithMessage("Password is required")
            .MinimumLength(8)
            .WithMessage("Password must be at least 8 characters long")
            .MaximumLength(50)
            .WithMessage("Password must be at most 50 characters long")
            .Equal(e => e.ConfirmPassword)
            .WithMessage("Passwords do not match");

        RuleFor(e => e.PhoneNumber)
            .NotEmpty()
            .WithMessage("Phone number is required");

        RuleFor(e => e.RoleName)
            .NotEmpty()
            .WithMessage("Role name is required")
            .MustAsync(async (roleName, cancellationToken) =>
                await roleManager.RoleExistsAsync(roleName))
            .WithMessage("Role does not exist");
    }
}