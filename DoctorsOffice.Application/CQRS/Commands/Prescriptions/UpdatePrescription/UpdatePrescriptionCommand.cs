using DoctorsOffice.Domain.DTO.Requests;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Utils;
using MediatR;

namespace DoctorsOffice.Application.CQRS.Commands.Prescriptions.UpdatePrescription;

public class UpdatePrescriptionCommand : IRequest<HttpResult<PrescriptionResponse>>
{
    public readonly IList<CreateDrugItemRequest>? DrugItems;
    public readonly DateTime? FulfillmentDeadline;
    public readonly Guid? PatientId;
    public readonly Guid PrescriptionId;

    public UpdatePrescriptionCommand(UpdatePrescriptionRequest request, Guid prescriptionId)
    {
        PrescriptionId = prescriptionId;
        PatientId = request.PatientId;
        DrugItems = request.DrugItems;
        FulfillmentDeadline = request.FulfillmentDeadline;
    }
}