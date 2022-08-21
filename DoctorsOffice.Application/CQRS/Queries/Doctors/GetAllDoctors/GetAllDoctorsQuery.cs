using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Utils;
using MediatR;

namespace DoctorsOffice.Application.CQRS.Queries.Doctors.GetAllDoctors;

public class GetAllDoctorsQuery : IRequest<HttpResult<IEnumerable<DoctorResponse>>>
{
}