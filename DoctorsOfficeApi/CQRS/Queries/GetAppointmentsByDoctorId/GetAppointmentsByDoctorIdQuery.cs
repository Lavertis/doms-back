using DoctorsOfficeApi.Models.Responses;
using MediatR;

namespace DoctorsOfficeApi.CQRS.Queries.GetAppointmentsByDoctorId;

public class GetAppointmentsByDoctorIdQuery : IRequest<IList<AppointmentResponse>>
{
    public string DoctorId { get; set; }

    public GetAppointmentsByDoctorIdQuery(string doctorId)
    {
        DoctorId = doctorId;
    }
}