using DoctorsOfficeApi.Repositories.PatientRepository;
using FluentValidation;

namespace DoctorsOfficeApi.Models.Requests;

public class CreatePrescriptionRequest
{
    public string Title { get; set; } = default!;
    public string Description { get; set; } = default!;
    public string PatientId { get; set; } = default!;
    public IList<Guid> DrugsIds { get; set; } = default!;
}

public class CreatePrescriptionRequestValidator : AbstractValidator<CreatePrescriptionRequest>
{
    public CreatePrescriptionRequestValidator(IPatientRepository patientRepository)
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Title is required");

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Description is required");

        RuleFor(x => x.PatientId)
            .NotEmpty()
            .WithMessage("PatientId is required")
            .MustAsync(async (id, cancellationToken) =>
            {
                if (!Guid.TryParse(id, out var guid))
                    return false;
                return await patientRepository.ExistsByIdAsync(guid);
            })
            .WithMessage("Patient with specified id does not exist");

        RuleFor(x => x.DrugsIds)
            .NotEmpty()
            .WithMessage("DrugsIds is required");
    }
}