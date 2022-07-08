using DoctorsOfficeApi.Data;
using DoctorsOfficeApi.Exceptions;
using DoctorsOfficeApi.Models.Responses;
using DoctorsOfficeApi.Services.AppointmentService;
using MediatR;

namespace DoctorsOfficeApi.CQRS.Commands.UpdateAppointment;

public class UpdateAppointmentHandler : IRequestHandler<UpdateAppointmentCommand, AppointmentResponse>
{
    private readonly AppDbContext _dbContext;
    private readonly IAppointmentService _appointmentService;

    public UpdateAppointmentHandler(AppDbContext dbContext, IAppointmentService appointmentService)
    {
        _dbContext = dbContext;
        _appointmentService = appointmentService;
    }

    public async Task<AppointmentResponse> Handle(UpdateAppointmentCommand request, CancellationToken cancellationToken)
    {
        var appointment = await _appointmentService.GetAppointmentByIdAsync(request.AppointmentId);
        appointment.Date = request.Date ?? appointment.Date;
        appointment.Description = request.Description ?? appointment.Description;
        try
        {
            if (request.Type != null)
                appointment.Type = await _appointmentService.GetAppointmentTypeByNameAsync(request.Type);
            if (request.Status != null)
                appointment.Status = await _appointmentService.GetAppointmentStatusByNameAsync(request.Status);
        }
        catch (NotFoundException e)
        {
            throw new BadRequestException(e.Message);
        }

        _dbContext.Appointments.Update(appointment);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new AppointmentResponse(appointment);
    }
}