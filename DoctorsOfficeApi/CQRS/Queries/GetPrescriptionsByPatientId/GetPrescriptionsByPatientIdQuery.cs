using DoctorsOfficeApi.Models.Responses;
using MediatR;

namespace DoctorsOfficeApi.CQRS.Queries.GetPrescriptionsByPatientId;

public class GetPrescriptionsByPatientIdQuery : IRequest<IList<PrescriptionResponse>>
{
    public readonly Guid PatientId;

    public GetPrescriptionsByPatientIdQuery(Guid patientId)
    {
        PatientId = patientId;
    }
}