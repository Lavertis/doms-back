using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DoctorsOffice.Application.CQRS.Queries.GetPrescriptionsByPatientId;

public class
    GetPrescriptionsByPatientIdHandler : IRequestHandler<GetPrescriptionsByPatientIdQuery, IList<PrescriptionResponse>>
{
    private readonly IPrescriptionRepository _prescriptionRepository;

    public GetPrescriptionsByPatientIdHandler(IPrescriptionRepository prescriptionRepository)
    {
        _prescriptionRepository = prescriptionRepository;
    }

    public async Task<IList<PrescriptionResponse>> Handle(GetPrescriptionsByPatientIdQuery request,
        CancellationToken cancellationToken)
    {
        var prescriptions = _prescriptionRepository.GetByPatientId(request.PatientId, p => p.DrugItems);
        var prescriptionResponses = await prescriptions
            .Select(p => new PrescriptionResponse(p))
            .ToListAsync(cancellationToken: cancellationToken);

        return prescriptionResponses;
    }
}