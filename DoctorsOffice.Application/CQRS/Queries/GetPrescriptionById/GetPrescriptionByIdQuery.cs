using DoctorsOffice.Domain.DTO.Responses;
using MediatR;

namespace DoctorsOffice.Application.CQRS.Queries.GetPrescriptionById;

public class GetPrescriptionByIdQuery : IRequest<PrescriptionResponse>
{
    public readonly Guid Id;

    public GetPrescriptionByIdQuery(Guid id)
    {
        Id = id;
    }
}