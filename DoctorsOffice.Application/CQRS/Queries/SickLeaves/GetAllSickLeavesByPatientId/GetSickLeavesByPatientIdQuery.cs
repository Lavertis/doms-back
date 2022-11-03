using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Filters;
using DoctorsOffice.Domain.Utils;
using DoctorsOffice.Domain.Wrappers;
using MediatR;

namespace DoctorsOffice.Application.CQRS.Queries.SickLeaves.GetAllSickLeavesByPatientId;

public class GetSickLeavesByPatientIdQuery : IRequest<HttpResult<PagedResponse<SickLeaveResponse>>>
{
    public PaginationFilter? PaginationFilter { get; set; }
    public Guid PatientId { get; set; }
}