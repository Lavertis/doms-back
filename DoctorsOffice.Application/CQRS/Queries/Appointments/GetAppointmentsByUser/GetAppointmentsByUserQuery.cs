using DoctorsOffice.Domain.DTO.Responses;
using MediatR;

namespace DoctorsOffice.Application.CQRS.Queries.Appointments.GetAppointmentsByUser;

public class GetAppointmentsByUserQuery : IRequest<IList<AppointmentResponse>>
{
    public readonly string Role;
    public readonly Guid UserId;

    public GetAppointmentsByUserQuery(Guid userId, string role)
    {
        UserId = userId;
        Role = role;
    }
}