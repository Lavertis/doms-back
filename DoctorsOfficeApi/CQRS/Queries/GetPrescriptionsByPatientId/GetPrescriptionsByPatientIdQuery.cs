using DoctorsOfficeApi.Models.Responses;
using MediatR;

namespace DoctorsOfficeApi.CQRS.Queries.GetPrescriptionsByPatientId;

public class GetPrescriptionsByPatientIdQuery : IRequest<IList<PrescriptionResponse>>
{
    public Guid PatientId { get; set; }

    public GetPrescriptionsByPatientIdQuery(Guid patientId)
    {
        PatientId = patientId;
    }
}