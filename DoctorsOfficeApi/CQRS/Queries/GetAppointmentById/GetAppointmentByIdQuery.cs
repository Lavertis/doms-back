using DoctorsOfficeApi.Models.Responses;
using MediatR;

namespace DoctorsOfficeApi.CQRS.Queries.GetAppointmentById;

public class GetAppointmentByIdQuery : IRequest<AppointmentResponse>
{
    public long Id { get; set; }

    public GetAppointmentByIdQuery(long id)
    {
        Id = id;
    }
}