using DoctorsOffice.Domain.DTO.Responses;
using MediatR;

namespace DoctorsOffice.Application.CQRS.Queries.GetPatientById;

public class GetPatientByIdQuery : IRequest<PatientResponse>
{
    public readonly Guid PatientId;

    public GetPatientByIdQuery(Guid patientId)
    {
        PatientId = patientId;
    }
}