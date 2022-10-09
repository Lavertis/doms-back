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
    private readonly IDrugItemRepository _drugItemRepository;
    private readonly IMapper _mapper;
    private readonly IPrescriptionRepository _prescriptionRepository;

    public CreatePrescriptionHandler(
        IPrescriptionRepository prescriptionRepository,
        IMapper mapper,
        IDrugItemRepository drugItemRepository)
    {
        _prescriptionRepository = prescriptionRepository;
        _mapper = mapper;
        _drugItemRepository = drugItemRepository;
    }

    public async Task<HttpResult<PrescriptionResponse>> Handle(
        CreatePrescriptionCommand request, CancellationToken cancellationToken)
    {
        var result = new HttpResult<PrescriptionResponse>();
        var newPrescription = new Prescription
        {
            PatientId = request.PatientId,
            DoctorId = request.DoctorId,
            AppointmentId = request.AppointmentId,
            FulfillmentDeadline = request.FulfillmentDeadline,
        };
        var prescriptionEntity = await _prescriptionRepository.CreateAsync(newPrescription);
        var drugItems = request.DrugItems.Select(drugItemRequest => new DrugItem
        {
            Dosage = drugItemRequest.Dosage,
            Name = drugItemRequest.Name,
            Quantity = drugItemRequest.Quantity,
            Rxcui = drugItemRequest.Rxcui,
            PrescriptionId = prescriptionEntity.Id
        }).ToList();
        var drugItemsEntities = await _drugItemRepository.CreateRangeAsync(drugItems);
        var drugItemResponses = drugItemsEntities
            .Select(drugItem => _mapper.Map<DrugItemResponse>(drugItem));
        var prescriptionResponse = _mapper.Map<PrescriptionResponse>(prescriptionEntity);
        prescriptionResponse.DrugItems = drugItemResponses;
        return result
            .WithValue(prescriptionResponse)
            .WithStatusCode(StatusCodes.Status201Created);
    }
}