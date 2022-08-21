using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Utils;
using MediatR;

namespace DoctorsOffice.Application.CQRS.Queries.Prescriptions.GetPrescriptionById;

public class GetPrescriptionByIdQuery : IRequest<HttpResult<PrescriptionResponse>>
{
    public readonly Guid PrescriptionId;

    public GetPrescriptionByIdQuery(Guid prescriptionId)
    {
        PrescriptionId = prescriptionId;
    }
}