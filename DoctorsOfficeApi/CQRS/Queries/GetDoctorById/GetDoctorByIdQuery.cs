using DoctorsOfficeApi.Models.Responses;
using MediatR;

namespace DoctorsOfficeApi.CQRS.Queries.GetDoctorById;

public class GetDoctorByIdQuery : IRequest<DoctorResponse>
{
    public string Id { get; set; }

    public GetDoctorByIdQuery(string id)
    {
        Id = id;
    }
}