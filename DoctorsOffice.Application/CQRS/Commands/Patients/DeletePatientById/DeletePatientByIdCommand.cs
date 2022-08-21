using DoctorsOffice.Domain.Utils;
using MediatR;

namespace DoctorsOffice.Application.CQRS.Commands.Patients.DeletePatientById;

public class DeletePatientByIdCommand : IRequest<HttpResult<Unit>>
{
    public readonly Guid PatientId;

    public DeletePatientByIdCommand(Guid patientId)
    {
        PatientId = patientId;
    }
}