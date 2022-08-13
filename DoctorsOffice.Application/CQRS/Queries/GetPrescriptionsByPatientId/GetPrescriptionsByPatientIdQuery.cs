using DoctorsOffice.Domain.DTO.Responses;
using MediatR;

namespace DoctorsOffice.Application.CQRS.Queries.GetPrescriptionsByPatientId;

public class GetPrescriptionsByPatientIdQuery : IRequest<IList<PrescriptionResponse>>
{
    public readonly Guid PatientId;

    public GetPrescriptionsByPatientIdQuery(Guid patientId)
    {
        PatientId = patientId;
    }
}