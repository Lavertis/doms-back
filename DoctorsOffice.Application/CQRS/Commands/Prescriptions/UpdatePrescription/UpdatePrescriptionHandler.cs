using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Entities;
using DoctorsOffice.Domain.Repositories;
using DoctorsOffice.Domain.Utils;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace DoctorsOffice.Application.CQRS.Commands.Prescriptions.UpdatePrescription;

public class UpdatePrescriptionHandler : IRequestHandler<UpdatePrescriptionCommand, HttpResult<PrescriptionResponse>>
{
    private readonly IPrescriptionRepository _prescriptionRepository;

    public UpdatePrescriptionHandler(IPrescriptionRepository prescriptionRepository)
    {
        _prescriptionRepository = prescriptionRepository;
    }

    public async Task<HttpResult<PrescriptionResponse>> Handle(
        UpdatePrescriptionCommand request, CancellationToken cancellationToken)
    {
        var result = new HttpResult<PrescriptionResponse>();

        var prescriptionToUpdate = await _prescriptionRepository.GetByIdAsync(request.PrescriptionId, p => p.DrugItems);
        if (prescriptionToUpdate is null)
        {
            return result
                .WithError(new Error {Message = $"Prescription with id {request.PrescriptionId} not found"})
                .WithStatusCode(StatusCodes.Status404NotFound);
        }

        prescriptionToUpdate.Title = request.Title ?? prescriptionToUpdate.Title;
        prescriptionToUpdate.Description = request.Description ?? prescriptionToUpdate.Description;
        prescriptionToUpdate.PatientId = request.PatientId ?? prescriptionToUpdate.PatientId;

        await _prescriptionRepository.UpdateByIdAsync(request.PrescriptionId, prescriptionToUpdate);
        if (request.DrugsIds is not null)
            await _prescriptionRepository.UpdateDrugItemsAsync(prescriptionToUpdate,
                request.DrugsIds!.Select(id => new DrugItem {Id = id}).ToList());

        var updatedPrescription = await _prescriptionRepository.GetByIdAsync(request.PrescriptionId, p => p.DrugItems);

        return result.WithValue(new PrescriptionResponse(updatedPrescription!));
    }
}