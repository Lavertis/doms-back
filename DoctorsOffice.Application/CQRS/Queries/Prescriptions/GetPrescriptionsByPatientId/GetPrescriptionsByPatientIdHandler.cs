using AutoMapper;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Repositories;
using DoctorsOffice.Domain.Utils;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DoctorsOffice.Application.CQRS.Queries.Prescriptions.GetPrescriptionsByPatientId;

public class
    GetPrescriptionsByPatientIdHandler
    : IRequestHandler<GetPrescriptionsByPatientIdQuery, HttpResult<IEnumerable<PrescriptionResponse>>>
{
    private readonly IMapper _mapper;
    private readonly IPrescriptionRepository _prescriptionRepository;

    public GetPrescriptionsByPatientIdHandler(IPrescriptionRepository prescriptionRepository, IMapper mapper)
    {
        _prescriptionRepository = prescriptionRepository;
        _mapper = mapper;
    }

    public async Task<HttpResult<IEnumerable<PrescriptionResponse>>> Handle(
        GetPrescriptionsByPatientIdQuery request, CancellationToken cancellationToken)
    {
        var result = new HttpResult<IEnumerable<PrescriptionResponse>>();

        var prescriptions = _prescriptionRepository.GetByPatientId(request.PatientId, p => p.DrugItems);
        var prescriptionResponses = await prescriptions
            .Select(p => _mapper.Map<PrescriptionResponse>(p))
            .ToListAsync(cancellationToken: cancellationToken);

        return result.WithValue(prescriptionResponses);
    }
}