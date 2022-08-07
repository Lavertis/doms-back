using DoctorsOfficeApi.Models.Responses;
using MediatR;

namespace DoctorsOfficeApi.CQRS.Queries.GetAppointmentById;

public class GetAppointmentByIdQuery : IRequest<AppointmentResponse>
{
    public readonly Guid AppointmentId;
    public readonly string RoleName;
    public readonly Guid UserId;

    public GetAppointmentByIdQuery(Guid appointmentId, Guid userId, string roleName)
    {
        AppointmentId = appointmentId;
        UserId = userId;
        RoleName = roleName;
    }
}