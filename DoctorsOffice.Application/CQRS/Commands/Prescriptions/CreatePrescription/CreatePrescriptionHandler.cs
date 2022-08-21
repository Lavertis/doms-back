using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Entities;
using DoctorsOffice.Domain.Repositories;
using DoctorsOffice.Domain.Utils;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace DoctorsOffice.Application.CQRS.Commands.Prescriptions.CreatePrescription;

public class CreatePrescriptionHandler : IRequestHandler<CreatePrescriptionCommand, HttpResult<PrescriptionResponse>>
{
    private readonly IPrescriptionRepository _prescriptionRepository;

    public CreatePrescriptionHandler(IPrescriptionRepository prescriptionRepository)
    {
        _prescriptionRepository = prescriptionRepository;
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
        return result
            .WithValue(new PrescriptionResponse(prescriptionEntity))
            .WithStatusCode(StatusCodes.Status201Created);
    }
}