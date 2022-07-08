using DoctorsOfficeApi.CQRS.Commands.CreateAppointment;
using DoctorsOfficeApi.Data;
using DoctorsOfficeApi.Entities;
using DoctorsOfficeApi.Exceptions;
using DoctorsOfficeApi.Models.Responses;
using DoctorsOfficeApi.Services.AppointmentService;
using MediatR;

namespace DoctorsOfficeApi.CQRS.Queries.CreateAppointment;

public class CreateAppointmentHandler : IRequestHandler<CreateAppointmentCommand, AppointmentResponse>
{
    private readonly AppDbContext _dbContext;
    private readonly IAppointmentService _appointmentService;

    public CreateAppointmentHandler(AppDbContext dbContext, IAppointmentService appointmentService)
    {
        _dbContext = dbContext;
        _appointmentService = appointmentService;
    }

    public async Task<AppointmentResponse> Handle(CreateAppointmentCommand request, CancellationToken cancellationToken)
    {
        Appointment appointment;
        try
        {
            appointment = new Appointment
            {
                Date = request.Date,
                Description = request.Description,
                Patient = (await _dbContext.Patients.FindAsync(
                    new object?[] { request.PatientId },
                    cancellationToken: cancellationToken))!, // TODO replace with patient service method
                Doctor = (await _dbContext.Doctors.FindAsync(
                    new object?[] { request.DoctorId },
                    cancellationToken: cancellationToken))!, // TODO replace with patient service method
                Status = await _appointmentService.GetAppointmentStatusByNameAsync(request.Status!),
                Type = await _appointmentService.GetAppointmentTypeByNameAsync(request.Type),
            };
        }
        catch (NotFoundException e)
        {
            throw new BadRequestException(e.Message);
        }

        _dbContext.Appointments.Add(appointment);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return new AppointmentResponse(appointment);
    }
}