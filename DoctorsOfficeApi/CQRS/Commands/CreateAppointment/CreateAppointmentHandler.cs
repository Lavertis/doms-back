using DoctorsOfficeApi.CQRS.Commands.CreateAppointment;
using DoctorsOfficeApi.Data;
using DoctorsOfficeApi.Entities;
using DoctorsOfficeApi.Exceptions;
using DoctorsOfficeApi.Models.Responses;
using DoctorsOfficeApi.Services.AppointmentService;
using DoctorsOfficeApi.Services.PatientService;
using MediatR;

namespace DoctorsOfficeApi.CQRS.Queries.CreateAppointment;

public class CreateAppointmentHandler : IRequestHandler<CreateAppointmentCommand, AppointmentResponse>
{
    private readonly AppDbContext _dbContext;
    private readonly IAppointmentService _appointmentService;
    private readonly IPatientService _patientService;

    public CreateAppointmentHandler(AppDbContext dbContext, IAppointmentService appointmentService, IPatientService patientService)
    {
        _dbContext = dbContext;
        _appointmentService = appointmentService;
        _patientService = patientService;
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
                Patient = (await _patientService.GetPatientByIdAsync(request.PatientId)),
                Doctor = (await _dbContext.Doctors.FindAsync(
                    new object?[] { request.DoctorId },
                    cancellationToken: cancellationToken))!, // TODO replace with doctor service method
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