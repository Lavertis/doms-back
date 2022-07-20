using DoctorsOfficeApi.Repositories.PatientRepository;
using FluentValidation;

namespace DoctorsOfficeApi.Models.Requests;

public class UpdatePrescriptionRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? PatientId { get; set; }
    public IList<Guid>? DrugsIds { get; set; }
}

public class UpdatePrescriptionRequestValidator : AbstractValidator<UpdatePrescriptionRequest>
{
    public UpdatePrescriptionRequestValidator(IPatientRepository patientRepository)
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Cannot be empty")
            .When(x => x.Title != null);

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Cannot be empty")
            .When(x => x.Description != null);

        RuleFor(x => x.PatientId)
            .MustAsync(async (id, cancellationToken) =>
            {
                if (!Guid.TryParse(id, out var guid))
                    return false;
                return await patientRepository.ExistsByIdAsync(guid);
            })
            .WithMessage("Patient with specified id does not exist")
            .When(x => x.PatientId is not null);

        RuleFor(x => x.DrugsIds)
            .NotEmpty()
            .WithMessage("Drugs ids must be specified")
            .When(x => x.DrugsIds is not null);
    }
}