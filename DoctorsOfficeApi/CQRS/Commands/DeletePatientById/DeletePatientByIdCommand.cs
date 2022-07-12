using MediatR;

namespace DoctorsOfficeApi.CQRS.Commands.DeletePatientById;

public class DeletePatientByIdCommand : IRequest<Unit>
{
    public DeletePatientByIdCommand(string id)
    {
        Id = id;
    }

    public string Id { get; set; }
}