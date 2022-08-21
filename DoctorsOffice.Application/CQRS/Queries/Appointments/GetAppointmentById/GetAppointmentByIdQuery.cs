using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Utils;
using MediatR;

namespace DoctorsOffice.Application.CQRS.Queries.Appointments.GetAppointmentById;

public class GetAppointmentByIdQuery : IRequest<HttpResult<AppointmentResponse>>
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