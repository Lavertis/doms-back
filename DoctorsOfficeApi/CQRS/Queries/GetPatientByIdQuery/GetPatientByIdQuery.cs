using DoctorsOfficeApi.Models.Responses;
using MediatR;

namespace DoctorsOfficeApi.CQRS.Queries.GetPatientByIdQuery;

public class GetPatientByIdQuery : IRequest<PatientResponse>
{
    public string Id { get; set; }

    public GetPatientByIdQuery(string id)
    {
        Id = id;
    }
}