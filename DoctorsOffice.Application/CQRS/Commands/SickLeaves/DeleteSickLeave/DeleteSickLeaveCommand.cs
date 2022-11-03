using DoctorsOffice.Domain.Utils;
using MediatR;

namespace DoctorsOffice.Application.CQRS.Commands.SickLeaves.DeleteSickLeave;

public class DeleteSickLeaveCommand : IRequest<HttpResult<Unit>>
{
    public Guid DoctorId { get; set; }
    public Guid SickLeaveId { get; set; }
}