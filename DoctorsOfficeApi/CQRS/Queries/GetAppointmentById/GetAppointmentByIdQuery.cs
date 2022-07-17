using DoctorsOfficeApi.Models.Responses;
using MediatR;

namespace DoctorsOfficeApi.CQRS.Queries.GetAppointmentById;

public class GetAppointmentByIdQuery : IRequest<AppointmentResponse>
{
    public Guid Id { get; set; }

    public GetAppointmentByIdQuery(Guid id)
    {
        Id = id;
    }
}