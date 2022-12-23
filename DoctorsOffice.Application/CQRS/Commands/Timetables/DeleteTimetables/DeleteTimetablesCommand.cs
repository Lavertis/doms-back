using DoctorsOffice.Domain.Utils;
using MediatR;

namespace DoctorsOffice.Application.CQRS.Commands.Timetables.DeleteTimetables;

public class DeleteTimetablesCommand : IRequest<HttpResult<Unit>>
{
    public readonly IEnumerable<Guid> Ids;

    public DeleteTimetablesCommand(IEnumerable<Guid> ids)
    {
        Ids = ids;
    }
}