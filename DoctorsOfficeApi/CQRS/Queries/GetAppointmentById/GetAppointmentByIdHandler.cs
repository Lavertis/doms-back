using DoctorsOfficeApi.Models.Responses;
using DoctorsOfficeApi.Services.AppointmentService;
using MediatR;

namespace DoctorsOfficeApi.CQRS.Queries.GetAppointmentById;

public class GetAppointmentByIdHandler : IRequestHandler<GetAppointmentByIdQuery, AppointmentResponse>
{
    private readonly IAppointmentService _appointmentService;


    public GetAppointmentByIdHandler(IAppointmentService appointmentService)
    {
        _appointmentService = appointmentService;
    }

    public async Task<AppointmentResponse> Handle(GetAppointmentByIdQuery request, CancellationToken cancellationToken)
    {
        var appointment = await _appointmentService.GetAppointmentByIdAsync(request.Id);

        return new AppointmentResponse(appointment);
    }
}