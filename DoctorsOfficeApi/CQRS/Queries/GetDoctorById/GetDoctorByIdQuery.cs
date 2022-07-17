using DoctorsOfficeApi.Models.Responses;
using MediatR;

namespace DoctorsOfficeApi.CQRS.Queries.GetDoctorById;

public class GetDoctorByIdQuery : IRequest<DoctorResponse>
{
    public Guid Id { get; set; }

    public GetDoctorByIdQuery(Guid id)
    {
        Id = id;
    }
}