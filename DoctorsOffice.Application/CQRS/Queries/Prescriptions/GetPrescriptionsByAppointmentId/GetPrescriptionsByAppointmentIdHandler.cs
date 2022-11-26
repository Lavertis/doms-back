using AutoMapper;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Repositories;
using DoctorsOffice.Domain.Utils;
using DoctorsOffice.Domain.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DoctorsOffice.Application.CQRS.Queries.Prescriptions.GetPrescriptionsByAppointmentId;

public class GetPrescriptionsByAppointmentIdHandler : IRequestHandler<GetPrescriptionsByAppointmentIdQuery,
    HttpResult<PagedResponse<PrescriptionResponse>>>
{
    private readonly IMapper _mapper;
    private readonly IPrescriptionRepository _prescriptionRepository;

    public GetPrescriptionsByAppointmentIdHandler(IPrescriptionRepository prescriptionRepository, IMapper mapper)
    {
        _prescriptionRepository = prescriptionRepository;
        _mapper = mapper;
    }

    public Task<HttpResult<PagedResponse<PrescriptionResponse>>> Handle(GetPrescriptionsByAppointmentIdQuery request,
        CancellationToken cancellationToken)
    {
        var prescriptionsQueryable = _prescriptionRepository.GetAll()
            .Include(prescription => prescription.DrugItems)
            .Where(prescription => prescription.AppointmentId == request.AppointmentId);

        if (request.DoctorId != null)
            prescriptionsQueryable = prescriptionsQueryable.Where(p => p.DoctorId == request.DoctorId);
        if (request.PatientId != null)
            prescriptionsQueryable = prescriptionsQueryable.Where(p => p.PatientId == request.PatientId);

        var prescriptionResponsesQueryable = prescriptionsQueryable.Select(p => _mapper.Map<PrescriptionResponse>(p));

        return Task.FromResult(
            PaginationUtils.CreatePagedHttpResult(prescriptionResponsesQueryable, request.PaginationFilter)
        );
    }
}