using DoctorsOfficeApi.Models.Responses;
using MediatR;

namespace DoctorsOfficeApi.CQRS.Queries.GetAppointmentsByUser;

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