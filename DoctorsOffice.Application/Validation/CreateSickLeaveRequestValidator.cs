using DoctorsOffice.Domain.DTO.Requests;
using DoctorsOffice.Domain.Repositories;
using FluentValidation;

namespace DoctorsOffice.Application.Validation;

public class CreateSickLeaveRequestValidator : AbstractValidator<CreateSickLeaveRequest>
{
    public CreateSickLeaveRequestValidator(IPatientRepository patientRepository)
    {
        CascadeMode = CascadeMode.Stop;

        RuleFor(x => x.PatientId)
            .NotEmpty()
            .WithMessage("PatientId is required")
            .MustAsync(async (id, _) => await patientRepository.ExistsByIdAsync(id))
            .WithMessage("Patient with specified id does not exist");

        RuleFor(x => x.DateEnd)
            .NotEmpty()
            .WithMessage("End date is required")
            .GreaterThanOrEqualTo(DateTime.UtcNow.AddDays(-1))
            .WithMessage("End date cannot be in the past")
            .GreaterThan(x => x.DateStart)
            .WithMessage("End date must be after start date");

        RuleFor(x => x.DateStart)
            .NotEmpty()
            .WithMessage("End date is required")
            .GreaterThanOrEqualTo(DateTime.UtcNow.AddDays(-1))
            .WithMessage("Start date cannot be in the past");
    }
}