using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Utils;
using MediatR;

namespace DoctorsOffice.Application.CQRS.Queries.AppointmentStatuses.GetAppointmentStatusById;

public class GetAppointmentStatusByIdQuery : IRequest<HttpResult<AppointmentStatusResponse>>
{
    public Guid Id { get; set; }

    public GetAppointmentStatusByIdQuery(Guid id)
    {
        Id = id;
    }
}