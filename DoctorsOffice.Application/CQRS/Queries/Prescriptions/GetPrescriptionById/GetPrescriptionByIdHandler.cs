using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Repositories;
using DoctorsOffice.Domain.Utils;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace DoctorsOffice.Application.CQRS.Queries.Prescriptions.GetPrescriptionById;

public class GetPrescriptionByIdHandler : IRequestHandler<GetPrescriptionByIdQuery, HttpResult<PrescriptionResponse>>
{
    private readonly IPrescriptionRepository _prescriptionRepository;

    public GetPrescriptionByIdHandler(IPrescriptionRepository prescriptionRepository)
    {
        _prescriptionRepository = prescriptionRepository;
    }

    public async Task<HttpResult<PrescriptionResponse>> Handle(GetPrescriptionByIdQuery request,
        CancellationToken cancellationToken)
    {
        var result = new HttpResult<PrescriptionResponse>();

        var prescription = await _prescriptionRepository.GetByIdAsync(request.PrescriptionId, p => p.DrugItems);
        if (prescription is null)
        {
            return result
                .WithError(new Error {Message = $"Prescription with id {request.PrescriptionId} not found"})
                .WithStatusCode(StatusCodes.Status404NotFound);
        }

        return result.WithValue(new PrescriptionResponse(prescription));
    }
}