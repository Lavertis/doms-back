using DoctorsOffice.Domain.DTO.Requests;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Utils;
using MediatR;

namespace DoctorsOffice.Application.CQRS.Commands.Prescriptions.CreatePrescription;

public class CreatePrescriptionCommand : IRequest<HttpResult<PrescriptionResponse>>
{
    public readonly Guid? AppointmentId;
    public readonly Guid DoctorId;
    public readonly IList<CreateDrugItemRequest> DrugItems;
    public readonly DateTime FulfillmentDeadline;
    public readonly Guid PatientId;

    public CreatePrescriptionCommand(CreatePrescriptionRequest request, Guid doctorId)
    {
        PatientId = request.PatientId;
        DrugItems = request.DrugItems;
        DoctorId = doctorId;
        AppointmentId = request.AppointmentId;
        FulfillmentDeadline = request.FulfillmentDeadline;
    }
}