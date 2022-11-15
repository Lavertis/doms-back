using DoctorsOffice.Domain.DTO.Requests;
using DoctorsOffice.Domain.Repositories;
using FluentValidation;

namespace DoctorsOffice.Application.CQRS.Commands.Prescriptions.CreatePrescription;

public class CreatePrescriptionRequestValidator : AbstractValidator<CreatePrescriptionRequest>
{
    public CreatePrescriptionRequestValidator(IPatientRepository patientRepository)
    {
        CascadeMode = CascadeMode.Stop;

        RuleFor(x => x.PatientId)
            .NotEmpty()
            .WithMessage("PatientId is required")
            .MustAsync(async (id, _) => await patientRepository.ExistsByIdAsync(id))
            .WithMessage("Patient with specified id does not exist");

        RuleFor(x => x.FulfillmentDeadline)
            .NotEmpty()
            .GreaterThanOrEqualTo(DateTime.UtcNow.AddDays(-1))
            .WithMessage("Fulfillment deadline cannot be in the past");

        RuleFor(x => x.DrugItems)
            .NotEmpty()
            .WithMessage("DrugItems are required");
    }
}