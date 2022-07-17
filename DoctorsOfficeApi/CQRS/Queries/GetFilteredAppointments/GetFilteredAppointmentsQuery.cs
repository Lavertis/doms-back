using DoctorsOfficeApi.Models.Responses;
using MediatR;

namespace DoctorsOfficeApi.CQRS.Queries.GetFilteredAppointments;

public class GetFilteredAppointmentsQuery : IRequest<IList<AppointmentResponse>>
{
    public DateTime? dateStart { get; set; }
    public DateTime? dateEnd { get; set; }
    public string? type { get; set; }
    public string? status { get; set; }
    public Guid? patientId { get; set; }
    public Guid? doctorId { get; set; }

    public GetFilteredAppointmentsQuery()
    {
    }

    public GetFilteredAppointmentsQuery(
        DateTime? dateStart,
        DateTime? dateEnd,
        string? type,
        string? status,
        string? patientId,
        string? doctorId)
    {
        this.dateStart = dateStart;
        this.dateEnd = dateEnd;
        this.type = type;
        this.status = status;
        this.doctorId = Guid.TryParse(doctorId, out var doctorGuid) ? doctorGuid : null;
        this.patientId = Guid.TryParse(patientId, out var patientGuid) ? patientGuid : null;
    }
}