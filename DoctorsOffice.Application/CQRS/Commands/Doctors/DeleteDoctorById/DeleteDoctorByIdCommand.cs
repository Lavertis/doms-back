using DoctorsOffice.Domain.Utils;
using MediatR;

namespace DoctorsOffice.Application.CQRS.Commands.Doctors.DeleteDoctorById;

public class DeleteDoctorByIdCommand : IRequest<HttpResult<Unit>>
{
    public readonly Guid DoctorId;

    public DeleteDoctorByIdCommand(Guid doctorId)
    {
        DoctorId = doctorId;
    }
}