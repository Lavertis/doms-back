using DoctorsOfficeApi.Data;
using DoctorsOfficeApi.Entities;
using DoctorsOfficeApi.Exceptions;
using DoctorsOfficeApi.Models.Responses;
using DoctorsOfficeApi.Services.AppointmentService;
using DoctorsOfficeApi.Services.DoctorService;
using DoctorsOfficeApi.Services.PatientService;
using MediatR;

namespace DoctorsOfficeApi.CQRS.Commands.CreateAppointment;

public class CreateAppointmentHandler : IRequestHandler<CreateAppointmentCommand, AppointmentResponse>
{
    private readonly AppDbContext _dbContext;
    private readonly IAppointmentService _appointmentService;
    private readonly IPatientService _patientService;
    private readonly IDoctorService _doctorService;

    public CreateAppointmentHandler(AppDbContext dbContext, IAppointmentService appointmentService, IPatientService patientService, IDoctorService doctorService)
    {
        _dbContext = dbContext;
        _appointmentService = appointmentService;
        _patientService = patientService;
        _doctorService = doctorService;
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
                Patient = await _patientService.GetPatientByIdAsync(request.PatientId),
                Doctor = await _doctorService.GetDoctorByIdAsync(request.DoctorId),
                Status = await _appointmentService.GetAppointmentStatusByNameAsync(request.Status),
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