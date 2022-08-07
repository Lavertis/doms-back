using MediatR;

namespace DoctorsOfficeApi.CQRS.Commands.DeleteDoctorById;

public class DeleteDoctorByIdCommand : IRequest<Unit>
{
    public readonly Guid DoctorId;

    public DeleteDoctorByIdCommand(Guid doctorId)
    {
        DoctorId = doctorId;
    }
}