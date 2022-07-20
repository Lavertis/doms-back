using DoctorsOfficeApi.Models.Responses;
using MediatR;

namespace DoctorsOfficeApi.CQRS.Queries.GetPrescriptionById;

public class GetPrescriptionByIdQuery : IRequest<PrescriptionResponse>
{
    public Guid Id { get; set; }

    public GetPrescriptionByIdQuery(Guid id)
    {
        Id = id;
    }
}