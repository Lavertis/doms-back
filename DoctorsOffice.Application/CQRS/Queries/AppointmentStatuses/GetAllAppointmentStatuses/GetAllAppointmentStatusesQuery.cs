using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Utils;
using MediatR;

namespace DoctorsOffice.Application.CQRS.Queries.AppointmentStatuses.GetAllAppointmentStatuses;

public class GetAllAppointmentStatusesQuery : IRequest<HttpResult<IEnumerable<AppointmentStatusResponse>>>
{
}