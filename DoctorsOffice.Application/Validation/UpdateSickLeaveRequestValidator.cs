using DoctorsOffice.Domain.DTO.Requests;
using DoctorsOffice.Domain.Repositories;
using FluentValidation;

namespace DoctorsOffice.Application.Validation;

public class UpdateSickLeaveRequestValidator : AbstractValidator<UpdateSickLeaveRequest>
{
    public UpdateSickLeaveRequestValidator(IPatientRepository patientRepository)
    {
        CascadeMode = CascadeMode.Stop;

        RuleFor(x => x.PatientId)
            .MustAsync(async (id, _) => await patientRepository.ExistsByIdAsync(id!.Value))
            .WithMessage("Patient with specified id does not exist")
            .When(x => x.PatientId is not null);

        RuleFor(x => x.DateEnd)
            .GreaterThanOrEqualTo(DateTime.UtcNow)
            .WithMessage("End date cannot be in the past")
            .When(x => x.DateStart is not null)
            .GreaterThan(x => x.DateStart)
            .WithMessage("End date must be after start date")
            .When(x => x.DateEnd is not null && x.DateStart is not null);

        RuleFor(x => x.DateStart)
            .GreaterThanOrEqualTo(DateTime.UtcNow)
            .WithMessage("Start date cannot be in the past")
            .When(x => x.DateStart is not null)
            .LessThan(x => x.DateEnd)
            .WithMessage("Start date must be before end date")
            .When(x => x.DateEnd is not null && x.DateStart is not null);
    }
}