using DoctorsOffice.Domain.DTO.Requests;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Utils;
using MediatR;

namespace DoctorsOffice.Application.CQRS.Queries.Appointments.GetFilteredAppointments;

public class GetFilteredAppointmentsQuery : IRequest<HttpResult<IEnumerable<AppointmentResponse>>>
{
    public readonly DateTime? DateEnd;
    public readonly DateTime? DateStart;
    public readonly Guid? DoctorId;
    public readonly Guid? PatientId;
    public readonly string? Status;
    public readonly string? Type;

    public GetFilteredAppointmentsQuery(GetAppointmentsFilteredRequest request)
    {
        DateStart = request.DateStart;
        DateEnd = request.DateEnd;
        Type = request.Type;
        Status = request.Status;
        PatientId = request.PatientId;
        DoctorId = request.DoctorId;
    }
}