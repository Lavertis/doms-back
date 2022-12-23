using DoctorsOffice.Domain.Repositories;
using DoctorsOffice.Domain.Utils;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace DoctorsOffice.Application.CQRS.Commands.Timetables.DeleteTimetables;

public class DeleteTimetablesHandler : IRequestHandler<DeleteTimetablesCommand, HttpResult<Unit>>
{
    private readonly ITimetableRepository _timetableRepository;

    public DeleteTimetablesHandler(ITimetableRepository timetableRepository)
    {
        _timetableRepository = timetableRepository;
    }

    public async Task<HttpResult<Unit>> Handle(DeleteTimetablesCommand request, CancellationToken cancellationToken)
    {
        var result = new HttpResult<Unit>();
        var timetablesToDelete = await _timetableRepository.GetAll()
            .Where(t => request.Ids.Contains(t.Id))
            .ToListAsync(cancellationToken: cancellationToken);

        if (timetablesToDelete.Count != request.Ids.Count())
        {
            var missingId = request.Ids.First(
                id => timetablesToDelete.FirstOrDefault(t => t.Id == id) is not null
            );
            return result
                .WithError(new Error { Message = $"Timetable with id {missingId} not found" })
                .WithStatusCode(StatusCodes.Status404NotFound);
        }

        await _timetableRepository.DeleteRange(timetablesToDelete);

        return result;
    }
}