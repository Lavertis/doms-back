using AutoMapper;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Repositories;
using DoctorsOffice.Domain.Utils;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace DoctorsOffice.Application.CQRS.Queries.AppointmentTypes.GetAppointmentTypeById;

public class
    GetAppointmentTypeByIdHandler : IRequestHandler<GetAppointmentTypeByIdQuery, HttpResult<AppointmentTypeResponse>>
{
    private readonly IAppointmentTypeRepository _appointmentTypeRepository;
    private readonly IMapper _mapper;

    public GetAppointmentTypeByIdHandler(IAppointmentTypeRepository appointmentTypeRepository, IMapper mapper)
    {
        _appointmentTypeRepository = appointmentTypeRepository;
        _mapper = mapper;
    }

    public async Task<HttpResult<AppointmentTypeResponse>> Handle(GetAppointmentTypeByIdQuery request,
        CancellationToken cancellationToken)
    {
        var result = new HttpResult<AppointmentTypeResponse>();

        var appointmentType = await _appointmentTypeRepository.GetByIdAsync(request.Id);
        var appointmentTypeResponse = _mapper.Map<AppointmentTypeResponse>(appointmentType);

        if (appointmentTypeResponse is null)
        {
            return result.WithStatusCode(StatusCodes.Status404NotFound);
        }

        return result.WithValue(appointmentTypeResponse);
    }
}