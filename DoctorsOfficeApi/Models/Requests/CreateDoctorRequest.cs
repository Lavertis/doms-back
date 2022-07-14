using DoctorsOfficeApi.Exceptions;
using DoctorsOfficeApi.Services.UserService;
using FluentValidation;

namespace DoctorsOfficeApi.Models.Requests;

public class CreateDoctorRequest
{
    public string UserName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string Password { get; set; } = default!;
    public string PhoneNumber { get; set; } = default!;
}

public class CreateDoctorRequestValidator : AbstractValidator<CreateDoctorRequest>
{
    [Obsolete("Obsolete")] // TODO replace OnFailure custom validator
    public CreateDoctorRequestValidator(IUserService userService)
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
            .OnFailure(request => throw new ConflictException("Username already exists"))
            .WithMessage("Username already exists");

        RuleFor(e => e.Email)
            .NotEmpty()
            .WithMessage("Email is required")
            .EmailAddress()
            .WithMessage("Email must be a valid email address")
            .MustAsync(async (email, cancellationToken) =>
                !await userService.EmailExistsAsync(email, cancellationToken))
            .OnFailure(request => throw new ConflictException("Email already exists"))
            .WithMessage("Email already exists");

        RuleFor(e => e.PhoneNumber)
            .NotEmpty()
            .WithMessage("Phone number is required");

        RuleFor(e => e.Password)
            .NotEmpty()
            .WithMessage("Password is required")
            .MinimumLength(8)
            .WithMessage("Password must be at least 8 characters long")
            .MaximumLength(50)
            .WithMessage("Password must be at most 50 characters long");
    }
}