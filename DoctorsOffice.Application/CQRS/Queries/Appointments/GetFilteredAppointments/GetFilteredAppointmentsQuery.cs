using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Filters;
using DoctorsOffice.Domain.Utils;
using DoctorsOffice.Domain.Wrappers;
using MediatR;

namespace DoctorsOffice.Application.CQRS.Queries.Appointments.GetFilteredAppointments;

public class GetFilteredAppointmentsQuery : IRequest<HttpResult<PagedResponse<AppointmentSearchResponse>>>
{
    public readonly PaginationFilter PaginationFilter;

    public GetFilteredAppointmentsQuery(PaginationFilter paginationFilter)
    {
        PaginationFilter = paginationFilter;
    }

    public DateTime? DateEnd { get; set; }
    public DateTime? DateStart { get; set; }
    public Guid? DoctorId { get; set; }
    public Guid? PatientId { get; set; }
    public string? Status { get; set; }
    public string? Type { get; set; }
}