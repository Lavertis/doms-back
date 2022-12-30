using AutoMapper;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Repositories;
using DoctorsOffice.Domain.Utils;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace DoctorsOffice.Application.CQRS.Queries.AppointmentStatuses.GetAppointmentStatusById;

public class
    GetAppointmentStatusByIdHandler : IRequestHandler<GetAppointmentStatusByIdQuery,
        HttpResult<AppointmentStatusResponse>>
{
    private readonly IAppointmentStatusRepository _appointmentStatusRepository;
    private readonly IMapper _mapper;

    public GetAppointmentStatusByIdHandler(IAppointmentStatusRepository appointmentStatusRepository, IMapper mapper)
    {
        _appointmentStatusRepository = appointmentStatusRepository;
        _mapper = mapper;
    }

    public async Task<HttpResult<AppointmentStatusResponse>> Handle(GetAppointmentStatusByIdQuery request,
        CancellationToken cancellationToken)
    {
        var result = new HttpResult<AppointmentStatusResponse>();

        var appointmentStatus = await _appointmentStatusRepository.GetByIdAsync(request.Id);
        var appointmentStatusResponse = _mapper.Map<AppointmentStatusResponse>(appointmentStatus);
        if (appointmentStatusResponse is null)
        {
            return result.WithStatusCode(StatusCodes.Status404NotFound);
        }

        return result.WithValue(appointmentStatusResponse);
    }
}