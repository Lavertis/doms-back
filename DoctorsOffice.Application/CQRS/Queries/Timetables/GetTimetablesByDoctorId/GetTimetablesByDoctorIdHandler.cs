using AutoMapper;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Repositories;
using DoctorsOffice.Domain.Utils;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DoctorsOffice.Application.CQRS.Queries.Timetables.GetTimetablesByDoctorId;

public class GetTimetablesByDoctorIdHandler : IRequestHandler<GetTimetablesByDoctorIdQuery, HttpResult<IEnumerable<TimetableResponse>>>
{
    private readonly IMapper _mapper;
    private readonly ITimetableRepository _timetableRepository;

    public GetTimetablesByDoctorIdHandler(ITimetableRepository timetableRepository, IMapper mapper)
    {
        _timetableRepository = timetableRepository;
        _mapper = mapper;
    }

    public async Task<HttpResult<IEnumerable<TimetableResponse>>> Handle(GetTimetablesByDoctorIdQuery request, CancellationToken cancellationToken)
    {
        var result = new HttpResult<IEnumerable<TimetableResponse>>();

        var timetablesQueryable = _timetableRepository.GetAll()
            .Where(t => t.DoctorId == request.DoctorId);

        if (request.StartDateTime is not null)
            timetablesQueryable = timetablesQueryable.Where(t => t.StartDateTime >= request.StartDateTime);
        if (request.EndDateTime is not null)
            timetablesQueryable = timetablesQueryable.Where(t => t.EndDateTime <= request.EndDateTime);

        var timetables = await timetablesQueryable
            .Select(t => _mapper.Map<TimetableResponse>(t))
            .ToListAsync(cancellationToken: cancellationToken);
        return result.WithValue(timetables);
    }
}