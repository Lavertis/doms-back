using AutoMapper;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Repositories;
using DoctorsOffice.Domain.Utils;
using DoctorsOffice.Domain.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DoctorsOffice.Application.CQRS.Queries.Prescriptions.GetPrescriptionsByPatientId;

public class
    GetPrescriptionsByPatientIdHandler
    : IRequestHandler<GetPrescriptionsByPatientIdQuery, HttpResult<PagedResponse<PrescriptionResponse>>>
{
    private readonly IMapper _mapper;
    private readonly IPrescriptionRepository _prescriptionRepository;

    public GetPrescriptionsByPatientIdHandler(IPrescriptionRepository prescriptionRepository, IMapper mapper)
    {
        _prescriptionRepository = prescriptionRepository;
        _mapper = mapper;
    }

    public Task<HttpResult<PagedResponse<PrescriptionResponse>>> Handle(
        GetPrescriptionsByPatientIdQuery request, CancellationToken cancellationToken)
    {
        var prescriptionResponsesQueryable = _prescriptionRepository.GetAll()
            .Include(p => p.DrugItems)
            .Where(p => p.PatientId == request.PatientId)
            .Select(p => _mapper.Map<PrescriptionResponse>(p));

        return Task.FromResult(
            PaginationUtils.CreatePagedHttpResult(prescriptionResponsesQueryable, request.PaginationFilter)
        );
    }
}