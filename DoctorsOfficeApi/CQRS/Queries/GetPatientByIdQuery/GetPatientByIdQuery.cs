using DoctorsOfficeApi.Models.Responses;
using MediatR;

namespace DoctorsOfficeApi.CQRS.Queries.GetPatientByIdQuery;

public class GetPatientByIdQuery : IRequest<PatientResponse>
{
    public readonly Guid PatientId;

    public GetPatientByIdQuery(Guid patientId)
    {
        PatientId = patientId;
    }
}