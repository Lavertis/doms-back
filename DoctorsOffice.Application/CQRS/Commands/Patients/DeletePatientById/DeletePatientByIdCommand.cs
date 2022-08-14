using MediatR;

namespace DoctorsOffice.Application.CQRS.Commands.Patients.DeletePatientById;

public class DeletePatientByIdCommand : IRequest<Unit>
{
    public readonly Guid PatientId;

    public DeletePatientByIdCommand(Guid patientId)
    {
        PatientId = patientId;
    }
}