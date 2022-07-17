using DoctorsOfficeApi.Models.Responses;
using MediatR;

namespace DoctorsOfficeApi.CQRS.Queries.GetAppointmentsByDoctorId;

public class GetAppointmentsByDoctorIdQuery : IRequest<IList<AppointmentResponse>>
{
    public Guid DoctorId { get; set; }

    public GetAppointmentsByDoctorIdQuery(Guid doctorId)
    {
        DoctorId = doctorId;
    }
}