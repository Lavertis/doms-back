using DoctorsOffice.Domain.DTO.Requests;
using DoctorsOffice.Domain.DTO.Responses;
using MediatR;

namespace DoctorsOffice.Application.CQRS.Commands.UpdatePrescription;

public class UpdatePrescriptionCommand : IRequest<PrescriptionResponse>
{
    public readonly string? Description;
    public readonly IList<Guid>? DrugsIds;
    public readonly Guid? PatientId;
    public readonly Guid PrescriptionId;
    public readonly string? Title;

    public UpdatePrescriptionCommand(UpdatePrescriptionRequest request, Guid prescriptionId)
    {
        PrescriptionId = prescriptionId;
        Title = request.Title;
        Description = request.Description;
        PatientId = request.PatientId;
        DrugsIds = request.DrugsIds;
    }
}