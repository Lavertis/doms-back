using DoctorsOfficeApi.Models.Requests;
using DoctorsOfficeApi.Models.Responses;
using MediatR;

namespace DoctorsOfficeApi.CQRS.Commands.UpdatePrescription;

public class UpdatePrescriptionCommand : IRequest<PrescriptionResponse>
{
    public Guid PrescriptionId { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public Guid? DoctorId { get; set; }
    public Guid? PatientId { get; set; }
    public IList<Guid>? DrugsIds { get; set; }

    public UpdatePrescriptionCommand()
    {
    }

    public UpdatePrescriptionCommand(Guid id, UpdatePrescriptionRequest request)
    {
        PrescriptionId = id;
        Title = request.Title;
        Description = request.Description;
        PatientId = request.PatientId is not null ? Guid.Parse(request.PatientId) : null;
        DrugsIds = request.DrugsIds;
    }
}