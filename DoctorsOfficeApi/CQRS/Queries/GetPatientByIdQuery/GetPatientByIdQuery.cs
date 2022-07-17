using DoctorsOfficeApi.Models.Responses;
using MediatR;

namespace DoctorsOfficeApi.CQRS.Queries.GetPatientByIdQuery;

public class GetPatientByIdQuery : IRequest<PatientResponse>
{
    public Guid Id { get; set; }

    public GetPatientByIdQuery(Guid id)
    {
        Id = id;
    }
}