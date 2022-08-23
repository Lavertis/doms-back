using AutoMapper;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Entities;
using DoctorsOffice.Domain.Repositories;
using DoctorsOffice.Domain.Utils;
using MediatR;
using Microsoft.AspNetCore.Http;

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
        var prescriptionResponse = _mapper.Map<PrescriptionResponse>(updatedPrescription);
        return result.WithValue(prescriptionResponse);
    }
}