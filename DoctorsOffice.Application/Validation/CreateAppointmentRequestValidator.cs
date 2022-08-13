using DoctorsOffice.Application.Services.User;
using DoctorsOffice.Domain.DTO.Requests;
using DoctorsOffice.Domain.Repositories;
using FluentValidation;

namespace DoctorsOffice.Application.Validation;

public class CreateAppointmentRequestValidator : AbstractValidator<CreateAppointmentRequest>
{
    public CreateAppointmentRequestValidator(
        IUserService userService,
        IAppointmentTypeRepository appointmentTypeRepository)
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
            .MustAsync((patientId, cancellationToken) => userService.UserExistsByIdAsync(patientId))
            .WithMessage("Patient does not exist");

        RuleFor(e => e.DoctorId)
            .NotEmpty()
            .WithMessage("DoctorId is required")
            .MustAsync((doctorId, cancellationToken) => userService.UserExistsByIdAsync(doctorId))
            .WithMessage("Doctor does not exist");

        RuleFor(e => e.Type)
            .NotEmpty()
            .WithMessage("Type is required")
            .MustAsync((type, cancellationToken) => appointmentTypeRepository.ExistsByNameAsync(type))
            .WithMessage("Type does not exist");
    }
}