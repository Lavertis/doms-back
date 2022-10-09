using DoctorsOffice.Domain.DTO.Requests;
using DoctorsOffice.Domain.Repositories;
using FluentValidation;

namespace DoctorsOffice.Application.Validation;

public class UpdatePrescriptionRequestValidator : AbstractValidator<UpdatePrescriptionRequest>
{
    public UpdatePrescriptionRequestValidator(IPatientRepository patientRepository)
    {
        CascadeMode = CascadeMode.Stop;

        RuleFor(x => x.PatientId)
            .MustAsync(async (id, _) => await patientRepository.ExistsByIdAsync(id!.Value))
            .WithMessage("Patient with specified id does not exist")
            .When(x => x.PatientId is not null);

        RuleFor(x => x.FulfillmentDeadline)
            .NotEmpty()
            .GreaterThanOrEqualTo(DateTime.UtcNow)
            .WithMessage("Fulfillment deadline cannot be in the past")
            .When(x => x.FulfillmentDeadline is not null);

        RuleFor(x => x.DrugItems)
            .NotEmpty()
            .WithMessage("DrugItems cannot be empty if not null")
            .When(x => x.DrugItems is not null);
    }
}