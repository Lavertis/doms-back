using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Filters;
using DoctorsOffice.Domain.Utils;
using DoctorsOffice.Domain.Wrappers;
using MediatR;

namespace DoctorsOffice.Application.CQRS.Queries.Appointments.GetAppointmentsByUser;

public class GetAppointmentsByUserQuery : IRequest<HttpResult<PagedResponse<AppointmentResponse>>>
{
    public PaginationFilter? PaginationFilter { get; set; }
    public string RoleName { get; set; } = null!;
    public Guid UserId { get; set; }
}