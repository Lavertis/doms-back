using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Entities;
using DoctorsOffice.Domain.Repositories;
using MediatR;

namespace DoctorsOffice.Application.CQRS.Commands.CreatePrescription;

public class CreatePrescriptionHandler : IRequestHandler<CreatePrescriptionCommand, PrescriptionResponse>
{
    private readonly IPrescriptionRepository _prescriptionRepository;

    public CreatePrescriptionHandler(IPrescriptionRepository prescriptionRepository)
    {
        _prescriptionRepository = prescriptionRepository;
    }

    public async Task<PrescriptionResponse> Handle(CreatePrescriptionCommand request,
        CancellationToken cancellationToken)
    {
        var newPrescription = new Prescription
        {
            Title = request.Title,
            Description = request.Description,
            PatientId = request.PatientId,
            DoctorId = request.DoctorId,
            DrugItems = request.DrugsIds.Select(id => new DrugItem {Id = id}).ToList()
        };
        var prescriptionEntity = await _prescriptionRepository.CreateAsync(newPrescription);
        return new PrescriptionResponse(prescriptionEntity);
    }
}