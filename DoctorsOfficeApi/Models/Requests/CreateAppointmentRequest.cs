using DoctorsOfficeApi.Services.AppointmentService;
using DoctorsOfficeApi.Services.UserService;
using FluentValidation;

namespace DoctorsOfficeApi.Models.Requests;

public class CreateAppointmentRequest
{
    public DateTime Date { get; set; }
    public string Description { get; set; } = default!;
    public string PatientId { get; set; } = default!;
    public string DoctorId { get; set; } = default!;
    public string Type { get; set; } = default!;
}

public class CreateAppointmentRequestValidator : AbstractValidator<CreateAppointmentRequest>
{
    public CreateAppointmentRequestValidator(IUserService userService, IAppointmentService appointmentService)
    {
        CascadeMode = CascadeMode.Stop;
        RuleFor(e => e.Date)
            .NotEmpty()
            .WithMessage("Username is required")
            .GreaterThanOrEqualTo(DateTime.UtcNow)
            .WithMessage("Date must be in the future");

        RuleFor(e => e.Description)
            .NotEmpty()
            .WithMessage("Description is required");

        RuleFor(e => e.PatientId)
            .NotEmpty()
            .WithMessage("PatientId is required")
            .MustAsync((patientId, cancellationToken) => userService.UserExistsByIdAsync(Guid.Parse(patientId)))
            .WithMessage("Patient does not exist");

        RuleFor(e => e.DoctorId)
            .NotEmpty()
            .WithMessage("DoctorId is required")
            .MustAsync(((doctorId, cancellationToken) => userService.UserExistsByIdAsync(Guid.Parse(doctorId))))
            .WithMessage("Doctor does not exist");

        RuleFor(e => e.Type)
            .NotEmpty()
            .WithMessage("Type is required")
            .MustAsync(((type, cancellationToken) => appointmentService.AppointmentTypeExistsAsync(type)))
            .WithMessage("Type does not exist");
    }
}