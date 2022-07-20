using DoctorsOfficeApi.Entities;
using DoctorsOfficeApi.Models.Responses;
using DoctorsOfficeApi.Repositories.PrescriptionRepository;
using MediatR;

namespace DoctorsOfficeApi.CQRS.Commands.CreatePrescription;

public class CreatePrescriptionHandler : IRequestHandler<CreatePrescriptionCommand, PrescriptionResponse>
{
    private readonly IPrescriptionRepository _prescriptionRepository;

    public CreatePrescriptionHandler(IPrescriptionRepository prescriptionRepository)
    {
        _prescriptionRepository = prescriptionRepository;
    }

    public async Task<PrescriptionResponse> Handle(CreatePrescriptionCommand request, CancellationToken cancellationToken)
    {
        var newPrescription = new Prescription
        {
            Title = request.Title,
            Description = request.Description,
            PatientId = request.PatientId,
            DoctorId = request.DoctorId,
            DrugItems = request.DrugsIds.Select(id => new DrugItem { Id = id }).ToList()
        };
        var prescriptionEntity = await _prescriptionRepository.CreateAsync(newPrescription);
        return new PrescriptionResponse(prescriptionEntity);
    }
}