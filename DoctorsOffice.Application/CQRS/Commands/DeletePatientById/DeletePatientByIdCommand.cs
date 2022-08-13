using MediatR;

namespace DoctorsOfficeApi.CQRS.Commands.DeletePatientById;

public class DeletePatientByIdCommand : IRequest<Unit>
{
    public readonly Guid PatientId;

    public DeletePatientByIdCommand(Guid patientId)
    {
        PatientId = patientId;
    }
}