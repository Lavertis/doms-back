using DoctorsOfficeApi.Models.Requests;
using DoctorsOfficeApi.Models.Responses;
using MediatR;

namespace DoctorsOfficeApi.CQRS.Queries.GetFilteredAppointments;

public class GetFilteredAppointmentsQuery : IRequest<IList<AppointmentResponse>>
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