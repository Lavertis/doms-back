using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Filters;
using DoctorsOffice.Domain.Utils;
using DoctorsOffice.Domain.Wrappers;
using MediatR;

namespace DoctorsOffice.Application.CQRS.Queries.SickLeaves.GetAllSickLeavesByDoctorId;

public class GetSickLeavesByDoctorIdQuery : IRequest<HttpResult<PagedResponse<SickLeaveResponse>>>
{
    public PaginationFilter? PaginationFilter { get; set; }
    public Guid DoctorId { get; set; }
}