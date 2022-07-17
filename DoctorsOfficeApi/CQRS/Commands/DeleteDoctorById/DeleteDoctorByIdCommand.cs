using MediatR;

namespace DoctorsOfficeApi.CQRS.Commands.DeleteDoctorById;

public class DeleteDoctorByIdCommand : IRequest<Unit>
{
    public Guid Id { get; set; }

    public DeleteDoctorByIdCommand(Guid id)
    {
        Id = id;
    }
}