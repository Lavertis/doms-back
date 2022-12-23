using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Utils;
using MediatR;

namespace DoctorsOffice.Application.CQRS.Queries.AppointmentTypes.GetAppointmentTypeById;

public class GetAppointmentTypeByIdQuery : IRequest<HttpResult<AppointmentTypeResponse>>
{
    public Guid Id { get; set; }

    public GetAppointmentTypeByIdQuery(Guid id)
    {
        Id = id;
    }
}