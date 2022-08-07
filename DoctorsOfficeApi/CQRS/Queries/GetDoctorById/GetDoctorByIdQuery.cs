using DoctorsOfficeApi.Models.Responses;
using MediatR;

namespace DoctorsOfficeApi.CQRS.Queries.GetDoctorById;

public class GetDoctorByIdQuery : IRequest<DoctorResponse>
{
    public readonly Guid DoctorId;

    public GetDoctorByIdQuery(Guid doctorId)
    {
        DoctorId = doctorId;
    }
}