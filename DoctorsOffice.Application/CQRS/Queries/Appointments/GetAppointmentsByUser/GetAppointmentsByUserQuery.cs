using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Utils;
using MediatR;

namespace DoctorsOffice.Application.CQRS.Queries.Appointments.GetAppointmentsByUser;

public class GetAppointmentsByUserQuery : IRequest<HttpResult<IEnumerable<AppointmentResponse>>>
{
    public readonly string Role;
    public readonly Guid UserId;

    public GetAppointmentsByUserQuery(Guid userId, string role)
    {
        UserId = userId;
        Role = role;
    }
}