using DoctorsOffice.Domain.DTO.Requests;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Filters;
using DoctorsOffice.Domain.Utils;
using DoctorsOffice.Domain.Wrappers;
using MediatR;

namespace DoctorsOffice.Application.CQRS.Queries.Appointments.GetFilteredAppointments;

public class GetFilteredAppointmentsQuery : IRequest<HttpResult<PagedResponse<AppointmentSearchResponse>>>
{
    public readonly DateTime? DateEnd;
    public readonly DateTime? DateStart;
    public readonly Guid? DoctorId;
    public readonly Guid? PatientId;
    public readonly string? Status;
    public readonly string? Type;
    public readonly PaginationFilter PaginationFilter;

    public GetFilteredAppointmentsQuery(
        GetAppointmentsFilteredRequest request,
        PaginationFilter paginationFilter)
    {
        DateStart = request.DateStart;
        DateEnd = request.DateEnd;
        Type = request.Type;
        Status = request.Status;
        PatientId = request.PatientId;
        DoctorId = request.DoctorId;
        PaginationFilter = paginationFilter;
    }
}