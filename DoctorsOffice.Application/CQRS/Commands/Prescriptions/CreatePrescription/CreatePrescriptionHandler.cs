using AutoMapper;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Entities;
using DoctorsOffice.Domain.Repositories;
using DoctorsOffice.Domain.Utils;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace DoctorsOffice.Application.CQRS.Commands.Prescriptions.CreatePrescription;

public class CreatePrescriptionHandler : IRequestHandler<CreatePrescriptionCommand, HttpResult<PrescriptionResponse>>
{
    private readonly IMapper _mapper;
    private readonly IPrescriptionRepository _prescriptionRepository;

    public CreatePrescriptionHandler(IPrescriptionRepository prescriptionRepository, IMapper mapper)
    {
        _prescriptionRepository = prescriptionRepository;
        _mapper = mapper;
    }

    public async Task<HttpResult<PrescriptionResponse>> Handle(
        CreatePrescriptionCommand request, CancellationToken cancellationToken)
    {
        var result = new HttpResult<PrescriptionResponse>();

        var newPrescription = new Prescription
        {
            Title = request.Title,
            Description = request.Description,
            PatientId = request.PatientId,
            DoctorId = request.DoctorId,
            DrugItems = request.DrugsIds.Select(id => new DrugItem {Id = id}).ToList()
        };
        var prescriptionEntity = await _prescriptionRepository.CreateAsync(newPrescription);
        var prescriptionResponse = _mapper.Map<PrescriptionResponse>(prescriptionEntity);
        return result
            .WithValue(prescriptionResponse)
            .WithStatusCode(StatusCodes.Status201Created);
    }
}