using DoctorsOffice.Domain.Repositories;
using DoctorsOffice.Domain.Utils;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace DoctorsOffice.Application.CQRS.Commands.Patients.DeletePatientById;

public class DeletePatientByIdHandler : IRequestHandler<DeletePatientByIdCommand, HttpResult<Unit>>
{
    private readonly IPatientRepository _patientRepository;

    public DeletePatientByIdHandler(IPatientRepository patientRepository)
    {
        _patientRepository = patientRepository;
    }

    public async Task<HttpResult<Unit>> Handle(DeletePatientByIdCommand request, CancellationToken cancellationToken)
    {
        var result = new HttpResult<Unit>();
        var patientDeleted = await _patientRepository.DeleteByIdAsync(request.PatientId);
        if (!patientDeleted)
        {
            return result
                .WithError(new Error {Message = $"Patient with id {request.PatientId} not found"})
                .WithStatusCode(StatusCodes.Status404NotFound);
        }

        return result.WithValue(Unit.Value);
    }
}