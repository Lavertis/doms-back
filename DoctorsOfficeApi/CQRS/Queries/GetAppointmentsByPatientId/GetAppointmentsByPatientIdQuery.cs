using DoctorsOfficeApi.Models.Responses;
using MediatR;

namespace DoctorsOfficeApi.CQRS.Queries.GetAppointmentsByPatientId;

public class GetAppointmentsByPatientIdQuery : IRequest<IList<AppointmentResponse>>
{
    public Guid PatientId { get; set; }

    public GetAppointmentsByPatientIdQuery(Guid patientId)
    {
        PatientId = patientId;
    }
}