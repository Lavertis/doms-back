using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Utils;
using MediatR;

namespace DoctorsOffice.Application.CQRS.Queries.Prescriptions.GetPrescriptionsByPatientId;

public class GetPrescriptionsByPatientIdQuery : IRequest<HttpResult<IEnumerable<PrescriptionResponse>>>
{
    public readonly Guid PatientId;

    public GetPrescriptionsByPatientIdQuery(Guid patientId)
    {
        PatientId = patientId;
    }
}