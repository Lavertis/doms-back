using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Repositories;
using MediatR;

namespace DoctorsOffice.Application.CQRS.Queries.GetPrescriptionById;

public class GetPrescriptionByIdHandler : IRequestHandler<GetPrescriptionByIdQuery, PrescriptionResponse>
{
    private readonly IPrescriptionRepository _prescriptionRepository;

    public GetPrescriptionByIdHandler(IPrescriptionRepository prescriptionRepository)
    {
        _prescriptionRepository = prescriptionRepository;
    }

    public async Task<PrescriptionResponse> Handle(GetPrescriptionByIdQuery request,
        CancellationToken cancellationToken)
    {
        var prescription = await _prescriptionRepository.GetByIdAsync(request.Id, p => p.DrugItems);
        return new PrescriptionResponse(prescription);
    }
}