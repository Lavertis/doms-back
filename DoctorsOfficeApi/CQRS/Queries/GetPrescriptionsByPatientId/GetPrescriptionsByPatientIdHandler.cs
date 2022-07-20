using DoctorsOfficeApi.Models.Responses;
using DoctorsOfficeApi.Repositories.PrescriptionRepository;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DoctorsOfficeApi.CQRS.Queries.GetPrescriptionsByPatientId;

public class GetPrescriptionsByPatientIdHandler : IRequestHandler<GetPrescriptionsByPatientIdQuery, IList<PrescriptionResponse>>
{
    private readonly IPrescriptionRepository _prescriptionRepository;

    public GetPrescriptionsByPatientIdHandler(IPrescriptionRepository prescriptionRepository)
    {
        _prescriptionRepository = prescriptionRepository;
    }

    public async Task<IList<PrescriptionResponse>> Handle(GetPrescriptionsByPatientIdQuery request, CancellationToken cancellationToken)
    {
        var prescriptions = _prescriptionRepository.GetByPatientId(request.PatientId, p => p.DrugItems);
        var prescriptionResponses = await prescriptions
            .Select(p => new PrescriptionResponse(p))
            .ToListAsync(cancellationToken: cancellationToken);

        return prescriptionResponses;
    }
}