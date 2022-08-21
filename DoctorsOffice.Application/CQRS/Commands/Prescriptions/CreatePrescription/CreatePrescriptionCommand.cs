using DoctorsOffice.Domain.DTO.Requests;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Utils;
using MediatR;

namespace DoctorsOffice.Application.CQRS.Commands.Prescriptions.CreatePrescription;

public class CreatePrescriptionCommand : IRequest<HttpResult<PrescriptionResponse>>
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