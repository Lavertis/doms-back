using AutoMapper;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Repositories;
using DoctorsOffice.Domain.Utils;
using DoctorsOffice.Domain.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DoctorsOffice.Application.CQRS.Queries.Prescriptions.GetPrescriptionsByDoctorId;

public class
    GetPrescriptionsByDoctorIdHandler
    : IRequestHandler<GetPrescriptionsByDoctorIdQuery, HttpResult<PagedResponse<PrescriptionResponse>>>
{
    private readonly IMapper _mapper;
    private readonly IPrescriptionRepository _prescriptionRepository;

    public GetPrescriptionsByDoctorIdHandler(IPrescriptionRepository prescriptionRepository, IMapper mapper)
    {
        _prescriptionRepository = prescriptionRepository;
        _mapper = mapper;
    }

    public Task<HttpResult<PagedResponse<PrescriptionResponse>>> Handle(
        GetPrescriptionsByDoctorIdQuery request, CancellationToken cancellationToken)
    {
        var prescriptionResponsesQueryable = _prescriptionRepository.GetAll()
            .Include(p => p.DrugItems)
            .Where(p => p.DoctorId == request.DoctorId)
            .Select(p => _mapper.Map<PrescriptionResponse>(p));

        return Task.FromResult(
            PaginationUtils.CreatePagedHttpResult(prescriptionResponsesQueryable, request.PaginationFilter)
        );
    }
}