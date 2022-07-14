using MediatR;

namespace DoctorsOfficeApi.CQRS.Commands.DeleteDoctorById;

public class DeleteDoctorByIdCommand : IRequest<Unit>
{
    public string Id { get; set; }

    public DeleteDoctorByIdCommand(string id)
    {
        Id = id;
    }
}