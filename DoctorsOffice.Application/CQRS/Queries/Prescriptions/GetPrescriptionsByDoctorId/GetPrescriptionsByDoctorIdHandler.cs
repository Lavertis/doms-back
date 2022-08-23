using AutoMapper;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Repositories;
using DoctorsOffice.Domain.Utils;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DoctorsOffice.Application.CQRS.Queries.Prescriptions.GetPrescriptionsByDoctorId;

public class
    GetPrescriptionsByDoctorIdHandler
    : IRequestHandler<GetPrescriptionsByDoctorIdQuery, HttpResult<IEnumerable<PrescriptionResponse>>>
{
    private readonly IMapper _mapper;
    private readonly IPrescriptionRepository _prescriptionRepository;

    public GetPrescriptionsByDoctorIdHandler(IPrescriptionRepository prescriptionRepository, IMapper mapper)
    {
        _prescriptionRepository = prescriptionRepository;
        _mapper = mapper;
    }

    public async Task<HttpResult<IEnumerable<PrescriptionResponse>>> Handle(
        GetPrescriptionsByDoctorIdQuery request, CancellationToken cancellationToken)
    {
        var result = new HttpResult<IEnumerable<PrescriptionResponse>>();

        var prescriptions = _prescriptionRepository.GetByDoctorId(request.DoctorId, p => p.DrugItems);
        var prescriptionResponses = await prescriptions
            .Select(p => _mapper.Map<PrescriptionResponse>(p))
            .ToListAsync(cancellationToken: cancellationToken);

        return result.WithValue(prescriptionResponses);
    }
}