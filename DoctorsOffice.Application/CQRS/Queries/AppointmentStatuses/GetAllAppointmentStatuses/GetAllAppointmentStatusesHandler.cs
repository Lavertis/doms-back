using AutoMapper;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Repositories;
using DoctorsOffice.Domain.Utils;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DoctorsOffice.Application.CQRS.Queries.AppointmentStatuses.GetAllAppointmentStatuses;

public class GetAllAppointmentStatusesHandler : IRequestHandler<GetAllAppointmentStatusesQuery, HttpResult<IEnumerable<AppointmentStatusResponse>>>
{
    private readonly IAppointmentStatusRepository _appointmentStatusRepository;
    private readonly IMapper _mapper;

    public GetAllAppointmentStatusesHandler(IAppointmentStatusRepository appointmentStatusRepository, IMapper mapper)
    {
        _appointmentStatusRepository = appointmentStatusRepository;
        _mapper = mapper;
    }

    public async Task<HttpResult<IEnumerable<AppointmentStatusResponse>>> Handle(GetAllAppointmentStatusesQuery request, CancellationToken cancellationToken)
    {
        var appointmentStatusResponses = await _appointmentStatusRepository
            .GetAll()
            .Select(appointmentStatus => _mapper.Map<AppointmentStatusResponse>(appointmentStatus))
            .ToListAsync(cancellationToken: cancellationToken);
        return new HttpResult<IEnumerable<AppointmentStatusResponse>>().WithValue(appointmentStatusResponses);
    }
}