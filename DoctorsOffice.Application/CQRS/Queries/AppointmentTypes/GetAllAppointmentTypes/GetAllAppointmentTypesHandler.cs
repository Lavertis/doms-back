using AutoMapper;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Repositories;
using DoctorsOffice.Domain.Utils;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DoctorsOffice.Application.CQRS.Queries.AppointmentTypes.GetAllAppointmentTypes;

public class GetAllAppointmentTypesHandler : IRequestHandler<GetAllAppointmentTypesQuery,
    HttpResult<IEnumerable<AppointmentTypeResponse>>>
{
    private readonly IAppointmentTypeRepository _appointmentTypeRepository;
    private readonly IMapper _mapper;

    public GetAllAppointmentTypesHandler(IAppointmentTypeRepository appointmentTypeRepository, IMapper mapper)
    {
        _appointmentTypeRepository = appointmentTypeRepository;
        _mapper = mapper;
    }

    public async Task<HttpResult<IEnumerable<AppointmentTypeResponse>>> Handle(GetAllAppointmentTypesQuery request,
        CancellationToken cancellationToken)
    {
        var appointmentTypeResponses = await _appointmentTypeRepository
            .GetAll()
            .Select(appointmentStatus => _mapper.Map<AppointmentTypeResponse>(appointmentStatus))
            .ToListAsync(cancellationToken: cancellationToken);

        return new HttpResult<IEnumerable<AppointmentTypeResponse>>().WithValue(appointmentTypeResponses);
    }
}