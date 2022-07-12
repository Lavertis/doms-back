using DoctorsOfficeApi.Exceptions;
using DoctorsOfficeApi.Services.UserService;
using FluentValidation;

namespace DoctorsOfficeApi.Models.Requests;

public class CreatePatientRequest
{
    public string UserName { get; set; } = default!;
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string PhoneNumber { get; set; } = default!;
    public string Address { get; set; } = default!;
    public DateTime DateOfBirth { get; set; }
    public string Password { get; set; } = default!;
    public string ConfirmPassword { get; set; } = default!;
}

public class CreatePatientRequestValidator : AbstractValidator<CreatePatientRequest>
{
    public CreatePatientRequestValidator(IUserService userService)
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
            .WithMessage("Password is required")
            .MinimumLength(8)
            .WithMessage("Password must be at least 8 characters long")
            .MaximumLength(50)
            .WithMessage("Password must be at most 50 characters long")
            .Equal(e => e.ConfirmPassword)
            .WithMessage("Passwords do not match");
    }
}