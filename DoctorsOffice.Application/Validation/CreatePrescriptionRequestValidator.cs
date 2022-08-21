using DoctorsOffice.Domain.DTO.Requests;
using DoctorsOffice.Domain.Repositories;
using FluentValidation;

namespace DoctorsOffice.Application.Validation;

public class CreatePrescriptionRequestValidator : AbstractValidator<CreatePrescriptionRequest>
{
    public CreatePrescriptionRequestValidator(IPatientRepository patientRepository)
    {
        CascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Title is required");

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Description is required");

        RuleFor(x => x.PatientId)
            .NotEmpty()
            .WithMessage("PatientId is required")
            .MustAsync(async (id, _) => await patientRepository.ExistsByIdAsync(id))
            .WithMessage("Patient with specified id does not exist");

        RuleFor(x => x.DrugsIds)
            .NotEmpty()
            .WithMessage("DrugIds is required");
    }
}