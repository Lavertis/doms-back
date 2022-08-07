using DoctorsOfficeApi.Models.Requests;
using DoctorsOfficeApi.Models.Responses;
using MediatR;

namespace DoctorsOfficeApi.CQRS.Commands.CreatePrescription;

public class CreatePrescriptionCommand : IRequest<PrescriptionResponse>
{
    public readonly string Description;
    public readonly Guid DoctorId;
    public readonly IList<Guid> DrugsIds;
    public readonly Guid PatientId;
    public readonly string Title;

    public CreatePrescriptionCommand(CreatePrescriptionRequest request, Guid doctorId)
    {
        Title = request.Title;
        Description = request.Description;
        PatientId = request.PatientId;
        DrugsIds = request.DrugsIds;
        DoctorId = doctorId;
    }
}