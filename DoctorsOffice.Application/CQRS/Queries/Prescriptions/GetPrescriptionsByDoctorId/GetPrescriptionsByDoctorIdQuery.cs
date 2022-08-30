using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Filters;
using DoctorsOffice.Domain.Utils;
using DoctorsOffice.Domain.Wrappers;
using MediatR;

namespace DoctorsOffice.Application.CQRS.Queries.Prescriptions.GetPrescriptionsByDoctorId;

public class GetPrescriptionsByDoctorIdQuery : IRequest<HttpResult<PagedResponse<PrescriptionResponse>>>
{
    public Guid DoctorId { get; set; }
    public PaginationFilter? PaginationFilter { get; set; }
}