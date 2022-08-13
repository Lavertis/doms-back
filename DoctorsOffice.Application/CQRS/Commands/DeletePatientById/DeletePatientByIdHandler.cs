using DoctorsOffice.Domain.Repositories;
using DoctorsOfficeApi.CQRS.Commands.DeletePatientById;
using MediatR;

namespace DoctorsOffice.Application.CQRS.Commands.DeletePatientById;

public class DeletePatientByIdHandler : IRequestHandler<DeletePatientByIdCommand, Unit>
{
    private readonly IPatientRepository _patientRepository;

    public DeletePatientByIdHandler(IPatientRepository patientRepository)
    {
        _patientRepository = patientRepository;
    }

    public async Task<Unit> Handle(DeletePatientByIdCommand request, CancellationToken cancellationToken)
    {
        await _patientRepository.DeleteByIdAsync(request.PatientId);
        return Unit.Value;
    }
}