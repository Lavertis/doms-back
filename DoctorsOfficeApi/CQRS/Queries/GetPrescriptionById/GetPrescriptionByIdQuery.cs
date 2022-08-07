using DoctorsOfficeApi.Models.Responses;
using MediatR;

namespace DoctorsOfficeApi.CQRS.Queries.GetPrescriptionById;

public class GetPrescriptionByIdQuery : IRequest<PrescriptionResponse>
{
    public readonly Guid Id;

    public GetPrescriptionByIdQuery(Guid id)
    {
        Id = id;
    }
}