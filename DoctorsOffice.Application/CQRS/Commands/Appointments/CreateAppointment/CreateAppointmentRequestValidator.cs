using DoctorsOffice.Domain.DTO.Requests;
using DoctorsOffice.Domain.Repositories;
using DoctorsOffice.Infrastructure.Identity;
using FluentValidation;

namespace DoctorsOffice.Application.CQRS.Commands.Appointments.CreateAppointment;

public class CreateAppointmentRequestValidator : AbstractValidator<CreateAppointmentRequest>
{
    public CreateAppointmentRequestValidator(
        AppUserManager appUserManager,
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
            .MustAsync((patientId, _) => appUserManager.ExistsByIdAsync(patientId))
            .WithMessage("Patient does not exist");

        RuleFor(e => e.DoctorId)
            .NotEmpty()
            .WithMessage("DoctorId is required")
            .MustAsync((doctorId, _) => appUserManager.ExistsByIdAsync(doctorId))
            .WithMessage("Doctor does not exist");

        RuleFor(e => e.TypeId)
            .NotEmpty()
            .WithMessage("Type is required")
            .MustAsync((typeId, _) => appointmentTypeRepository.ExistsByIdAsync(typeId))
            .WithMessage("Type does not exist");
    }
}