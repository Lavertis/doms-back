using DoctorsOfficeApi.Models.Responses;
using MediatR;

namespace DoctorsOfficeApi.CQRS.Queries.GetAppointmentsByPatientId;

public class GetAppointmentsByPatientIdQuery : IRequest<IList<AppointmentResponse>>
{
    public string PatientId { get; set; }

    public GetAppointmentsByPatientIdQuery(string patientId)
    {
        PatientId = patientId;
    }
}