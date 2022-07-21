using DoctorsOfficeApi.Entities;
using DoctorsOfficeApi.Models.Responses;
using DoctorsOfficeApi.Repositories.PrescriptionRepository;
using MediatR;

namespace DoctorsOfficeApi.CQRS.Commands.UpdatePrescription;

public class UpdatePrescriptionHandler : IRequestHandler<UpdatePrescriptionCommand, PrescriptionResponse>
{
    private readonly IPrescriptionRepository _prescriptionRepository;

    public UpdatePrescriptionHandler(IPrescriptionRepository prescriptionRepository)
    {
        _prescriptionRepository = prescriptionRepository;
    }

    public async Task<PrescriptionResponse> Handle(UpdatePrescriptionCommand request, CancellationToken cancellationToken)
    {
        var prescriptionToUpdate = await _prescriptionRepository.GetByIdAsync(request.PrescriptionId, p => p.DrugItems);
        prescriptionToUpdate.Title = request.Title ?? prescriptionToUpdate.Title;
        prescriptionToUpdate.Description = request.Description ?? prescriptionToUpdate.Description;
        prescriptionToUpdate.PatientId = request.PatientId ?? prescriptionToUpdate.PatientId;
        prescriptionToUpdate.DoctorId = request.DoctorId ?? prescriptionToUpdate.DoctorId;

        await _prescriptionRepository.UpdateByIdAsync(request.PrescriptionId, prescriptionToUpdate);
        if (request.DrugsIds is not null)
            await _prescriptionRepository.UpdateDrugItemsAsync(prescriptionToUpdate, request.DrugsIds!.Select(id => new DrugItem { Id = id }).ToList());

        var updatedPrescription = await _prescriptionRepository.GetByIdAsync(request.PrescriptionId, p => p.DrugItems);
        return new PrescriptionResponse(updatedPrescription);
    }
}