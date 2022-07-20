using DoctorsOfficeApi.Models.Responses;
using DoctorsOfficeApi.Repositories.PrescriptionRepository;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DoctorsOfficeApi.CQRS.Queries.GetPrescriptionsByDoctorId;

public class GetPrescriptionsByDoctorIdHandler : IRequestHandler<GetPrescriptionsByDoctorIdQuery, IList<PrescriptionResponse>>
{
    private readonly IPrescriptionRepository _prescriptionRepository;

    public GetPrescriptionsByDoctorIdHandler(IPrescriptionRepository prescriptionRepository)
    {
        _prescriptionRepository = prescriptionRepository;
    }

    public async Task<IList<PrescriptionResponse>> Handle(GetPrescriptionsByDoctorIdQuery request, CancellationToken cancellationToken)
    {
        var prescriptions = _prescriptionRepository.GetByDoctorId(request.DoctorId, p => p.DrugItems);
        var prescriptionResponses = await prescriptions
            .Select(p => new PrescriptionResponse(p))
            .ToListAsync(cancellationToken: cancellationToken);

        return prescriptionResponses;
    }
}