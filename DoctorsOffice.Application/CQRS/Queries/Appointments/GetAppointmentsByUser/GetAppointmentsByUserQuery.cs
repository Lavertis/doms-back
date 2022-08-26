using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Utils;
using MediatR;

namespace DoctorsOffice.Application.CQRS.Queries.Appointments.GetAppointmentsByUser;

public class GetAppointmentsByUserQuery : IRequest<HttpResult<IEnumerable<AppointmentResponse>>>
{
    public string RoleName { get; set; } = null!;
    public Guid UserId { get; set; }
}