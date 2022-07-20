using DoctorsOfficeApi.Models.Responses;
using MediatR;

namespace DoctorsOfficeApi.CQRS.Commands.CreatePrescription;

public class CreatePrescriptionCommand : IRequest<PrescriptionResponse>
{
    public string Title { get; set; } = default!;
    public string Description { get; set; } = default!;
    public Guid DoctorId { get; set; }
    public Guid PatientId { get; set; }
    public IList<Guid> DrugsIds { get; set; } = default!;


    public CreatePrescriptionCommand()
    {
    }

    public CreatePrescriptionCommand(string title, string description, Guid doctorId, Guid patientId, IList<Guid> drugsIds)
    {
        Title = title;
        Description = description;
        DoctorId = doctorId;
        PatientId = patientId;
        DrugsIds = drugsIds;
    }
}