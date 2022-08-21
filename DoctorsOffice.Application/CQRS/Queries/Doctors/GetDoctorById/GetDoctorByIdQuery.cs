using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Utils;
using MediatR;

namespace DoctorsOffice.Application.CQRS.Queries.Doctors.GetDoctorById;

public class GetDoctorByIdQuery : IRequest<HttpResult<DoctorResponse>>
{
    public readonly Guid DoctorId;

    public GetDoctorByIdQuery(Guid doctorId)
    {
        DoctorId = doctorId;
    }
}