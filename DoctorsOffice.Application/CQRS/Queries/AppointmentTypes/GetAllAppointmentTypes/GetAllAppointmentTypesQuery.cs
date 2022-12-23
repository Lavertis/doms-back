using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Utils;
using MediatR;

namespace DoctorsOffice.Application.CQRS.Queries.AppointmentTypes.GetAllAppointmentTypes;

public class GetAllAppointmentTypesQuery : IRequest<HttpResult<IEnumerable<AppointmentTypeResponse>>>
{
}