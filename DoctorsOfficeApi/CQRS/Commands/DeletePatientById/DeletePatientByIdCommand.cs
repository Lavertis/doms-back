using MediatR;

namespace DoctorsOfficeApi.CQRS.Commands.DeletePatientById;

public class DeletePatientByIdCommand : IRequest<Unit>
{
    public Guid Id { get; set; }

    public DeletePatientByIdCommand(Guid id)
    {
        Id = id;
    }
}