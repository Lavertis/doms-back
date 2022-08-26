using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Utils;
using MediatR;

namespace DoctorsOffice.Application.CQRS.Queries.Appointments.GetAppointmentById;

public class GetAppointmentByIdQuery : IRequest<HttpResult<AppointmentResponse>>
{
    public Guid AppointmentId { get; set; }
    public string RoleName { get; set; } = null!;
    public Guid UserId { get; set; }
}