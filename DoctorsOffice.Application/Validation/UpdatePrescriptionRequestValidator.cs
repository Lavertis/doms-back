using DoctorsOffice.Domain.DTO.Requests;
using DoctorsOffice.Domain.Repositories;
using FluentValidation;

namespace DoctorsOffice.Application.Validation;

public class UpdatePrescriptionRequestValidator : AbstractValidator<UpdatePrescriptionRequest>
{
    public UpdatePrescriptionRequestValidator(IPatientRepository patientRepository)
    {
        CascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Cannot be empty")
            .When(x => x.Title != null);

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Cannot be empty")
            .When(x => x.Description != null);

        RuleFor(x => x.PatientId)
            .MustAsync(async (id, cancellationToken) => await patientRepository.ExistsByIdAsync(id!.Value))
            .WithMessage("Patient with specified id does not exist")
            .When(x => x.PatientId is not null);

        RuleFor(x => x.DrugsIds)
            .NotEmpty()
            .WithMessage("Drugs ids must be specified")
            .When(x => x.DrugsIds is not null);
    }
}