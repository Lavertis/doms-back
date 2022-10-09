using AutoMapper;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Entities;
using DoctorsOffice.Domain.Repositories;
using DoctorsOffice.Domain.Utils;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace DoctorsOffice.Application.CQRS.Commands.Prescriptions.UpdatePrescription;

public class UpdatePrescriptionHandler : IRequestHandler<UpdatePrescriptionCommand, HttpResult<PrescriptionResponse>>
{
    private readonly IMapper _mapper;
    private readonly IPrescriptionRepository _prescriptionRepository;

    public UpdatePrescriptionHandler(IPrescriptionRepository prescriptionRepository, IMapper mapper)
    {
        _prescriptionRepository = prescriptionRepository;
        _mapper = mapper;
    }

    public async Task<HttpResult<PrescriptionResponse>> Handle(
        UpdatePrescriptionCommand request, CancellationToken cancellationToken)
    {
        var result = new HttpResult<PrescriptionResponse>();

        var prescriptionToUpdate = await _prescriptionRepository.GetAll()
            .Include(prescription => prescription.DrugItems)
            .FirstOrDefaultAsync(prescription => prescription.Id == request.PrescriptionId, cancellationToken);

        if (prescriptionToUpdate is null)
        {
            return result
                .WithError(new Error {Message = $"Prescription with id {request.PrescriptionId} not found"})
                .WithStatusCode(StatusCodes.Status404NotFound);
        }

        prescriptionToUpdate.PatientId = request.PatientId ?? prescriptionToUpdate.PatientId;
        prescriptionToUpdate.FulfillmentDeadline =
            request.FulfillmentDeadline ?? prescriptionToUpdate.FulfillmentDeadline;

        await _prescriptionRepository.UpdateAsync(prescriptionToUpdate);
        if (request.DrugItems is not null)
        {
            var drugItems = request.DrugItems
                .Select(d => new DrugItem {Rxcui = d.Rxcui, Dosage = d.Dosage, Name = d.Name, Quantity = d.Quantity})
                .ToList();
            await _prescriptionRepository.UpdateDrugItemsAsync(prescriptionToUpdate, drugItems);
        }

        var updatedPrescription = await _prescriptionRepository.GetAll()
            .Include(prescription => prescription.DrugItems)
            .FirstOrDefaultAsync(prescription => prescription.Id == request.PrescriptionId, cancellationToken);

        var prescriptionResponse = _mapper.Map<PrescriptionResponse>(updatedPrescription);
        return result.WithValue(prescriptionResponse);
    }
}