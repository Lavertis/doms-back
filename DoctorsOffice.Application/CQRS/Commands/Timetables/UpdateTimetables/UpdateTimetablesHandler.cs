using AutoMapper;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Repositories;
using DoctorsOffice.Domain.Utils;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace DoctorsOffice.Application.CQRS.Commands.Timetables.UpdateTimetables;

public class
    UpdateTimetablesHandler : IRequestHandler<UpdateTimetablesCommand, HttpResult<IEnumerable<TimetableResponse>>>
{
    private readonly IMapper _mapper;
    private readonly ITimetableRepository _timetableRepository;

    public UpdateTimetablesHandler(ITimetableRepository timetableRepository, IMapper mapper)
    {
        _timetableRepository = timetableRepository;
        _mapper = mapper;
    }

    public async Task<HttpResult<IEnumerable<TimetableResponse>>> Handle(UpdateTimetablesCommand request,
        CancellationToken cancellationToken)
    {
        var result = new HttpResult<IEnumerable<TimetableResponse>>();

        var updateRequests = request.Data.ToList();
        var timetableToUpdateIds = updateRequests.Select(updateRequest => updateRequest.Id);

        var timetablesToUpdate = await _timetableRepository.GetAll()
            .Where(t => timetableToUpdateIds.Contains(t.Id))
            .ToListAsync(cancellationToken: cancellationToken);

        if (timetablesToUpdate.Count != updateRequests.Count)
        {
            var missingRequest = updateRequests.First(
                r => timetablesToUpdate.FirstOrDefault(t => t.Id == r.Id) is not null
            );
            return result
                .WithError(new Error {Message = $"Timetable with id {missingRequest.Id} not found"})
                .WithStatusCode(StatusCodes.Status404NotFound);
        }

        timetablesToUpdate.ForEach(t =>
        {
            var updateRequest = updateRequests.First(r => r.Id == t.Id);
            t.StartDateTime = updateRequest.StartDateTime;
            t.EndDateTime = updateRequest.EndDateTime;
        });

        var updatedTimetables = await _timetableRepository.UpdateRangeAsync(timetablesToUpdate);
        var updatedTimetableResponses = updatedTimetables
            .Select(t => _mapper.Map<TimetableResponse>(t));

        return result.WithValue(updatedTimetableResponses);
    }
}