using AutoMapper;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Entities;
using DoctorsOffice.Domain.Repositories;
using DoctorsOffice.Domain.Utils;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace DoctorsOffice.Application.CQRS.Commands.Timetables.CreateTimetables;

public class
    CreateTimetablesHandler : IRequestHandler<CreateTimetablesCommand, HttpResult<IEnumerable<TimetableResponse>>>
{
    private readonly IMapper _mapper;
    private readonly ITimetableRepository _timetableRepository;

    public CreateTimetablesHandler(ITimetableRepository timetableRepository, IMapper mapper)
    {
        _timetableRepository = timetableRepository;
        _mapper = mapper;
    }

    public async Task<HttpResult<IEnumerable<TimetableResponse>>> Handle(CreateTimetablesCommand request,
        CancellationToken cancellationToken)
    {
        var result = new HttpResult<IEnumerable<TimetableResponse>>();

        var newTimetables = request.Data.Select(createTimetableRequest => new Timetable
        {
            StartDateTime = createTimetableRequest.StartDateTime,
            EndDateTime = createTimetableRequest.EndDateTime,
            DoctorId = request.DoctorId
        });
        var createdTimetables = await _timetableRepository.CreateRangeAsync(newTimetables.ToList());
        var timetableResponses = createdTimetables
            .Select(timetable => _mapper.Map<TimetableResponse>(timetable));

        return result
            .WithValue(timetableResponses)
            .WithStatusCode(StatusCodes.Status201Created);
    }
}