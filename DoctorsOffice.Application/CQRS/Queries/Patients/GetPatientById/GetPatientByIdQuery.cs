using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Utils;
using MediatR;

namespace DoctorsOffice.Application.CQRS.Queries.Patients.GetPatientById;

public class GetPatientByIdQuery : IRequest<HttpResult<PatientResponse>>
{
    public readonly Guid PatientId;

    public GetPatientByIdQuery(Guid patientId)
    {
        PatientId = patientId;
    }
}