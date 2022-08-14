using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Entities;
using DoctorsOffice.Domain.Repositories;
using MediatR;

namespace DoctorsOffice.Application.CQRS.Commands.Prescriptions.UpdatePrescription;

public class UpdatePrescriptionHandler : IRequestHandler<UpdatePrescriptionCommand, PrescriptionResponse>
{
    private readonly IPrescriptionRepository _prescriptionRepository;

    public UpdatePrescriptionHandler(IPrescriptionRepository prescriptionRepository)
    {
        _prescriptionRepository = prescriptionRepository;
    }

    public async Task<PrescriptionResponse> Handle(UpdatePrescriptionCommand request,
        CancellationToken cancellationToken)
    {
        var prescriptionToUpdate = await _prescriptionRepository.GetByIdAsync(request.PrescriptionId, p => p.DrugItems);
        prescriptionToUpdate.Title = request.Title ?? prescriptionToUpdate.Title;
        prescriptionToUpdate.Description = request.Description ?? prescriptionToUpdate.Description;
        prescriptionToUpdate.PatientId = request.PatientId ?? prescriptionToUpdate.PatientId;

        await _prescriptionRepository.UpdateByIdAsync(request.PrescriptionId, prescriptionToUpdate);
        if (request.DrugsIds is not null)
            await _prescriptionRepository.UpdateDrugItemsAsync(prescriptionToUpdate,
                request.DrugsIds!.Select(id => new DrugItem {Id = id}).ToList());

        var updatedPrescription = await _prescriptionRepository.GetByIdAsync(request.PrescriptionId, p => p.DrugItems);
        return new PrescriptionResponse(updatedPrescription);
    }
}