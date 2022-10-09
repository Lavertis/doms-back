using DoctorsOffice.Domain.Repositories;
using DoctorsOffice.Domain.Utils;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace DoctorsOffice.Application.CQRS.Commands.Prescriptions.DeletePrescription;

public class DeletePrescriptionHandler : IRequestHandler<DeletePrescriptionCommand, HttpResult<Unit>>
{
    private readonly IPrescriptionRepository _prescriptionRepository;

    public DeletePrescriptionHandler(IPrescriptionRepository prescriptionRepository)
    {
        _prescriptionRepository = prescriptionRepository;
    }

    public async Task<HttpResult<Unit>> Handle(DeletePrescriptionCommand request, CancellationToken cancellationToken)
    {
        var result = new HttpResult<Unit>();
        var prescription = await _prescriptionRepository.GetByIdAsync(request.PrescriptionId);
        if (prescription == null)
        {
            return result
                .WithError(new Error {Message = $"Prescription with id {request.PrescriptionId} not found"})
                .WithStatusCode(StatusCodes.Status404NotFound);
        }

        if (prescription.DoctorId != request.DoctorId)
        {
            return result
                .WithError(new Error {Message = "You are not allowed to delete this prescription"})
                .WithStatusCode(StatusCodes.Status403Forbidden);
        }

        await _prescriptionRepository.DeleteByIdAsync(request.PrescriptionId);
        return result.WithValue(Unit.Value);
    }
}