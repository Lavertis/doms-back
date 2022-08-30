using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Filters;
using DoctorsOffice.Domain.Utils;
using DoctorsOffice.Domain.Wrappers;
using MediatR;

namespace DoctorsOffice.Application.CQRS.Queries.Prescriptions.GetPrescriptionsByPatientId;

public class GetPrescriptionsByPatientIdQuery : IRequest<HttpResult<PagedResponse<PrescriptionResponse>>>
{
    public PaginationFilter? PaginationFilter { get; set; }
    public Guid PatientId { get; set; }
}