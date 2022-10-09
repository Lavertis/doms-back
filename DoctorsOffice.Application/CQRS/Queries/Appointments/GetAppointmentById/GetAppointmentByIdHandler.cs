using AutoMapper;
using DoctorsOffice.Application.Services.Appointments;
using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Repositories;
using DoctorsOffice.Domain.Utils;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace DoctorsOffice.Application.CQRS.Queries.Appointments.GetAppointmentById;

public class GetAppointmentByIdHandler : IRequestHandler<GetAppointmentByIdQuery, HttpResult<AppointmentResponse>>
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IAppointmentService _appointmentService;
    private readonly IMapper _mapper;

    public GetAppointmentByIdHandler(
        IAppointmentRepository appointmentRepository,
        IAppointmentService appointmentService,
        IMapper mapper)
    {
        _appointmentRepository = appointmentRepository;
        _appointmentService = appointmentService;
        _mapper = mapper;
    }

    public async Task<HttpResult<AppointmentResponse>> Handle(
        GetAppointmentByIdQuery request, CancellationToken cancellationToken)
    {
        var result = new HttpResult<AppointmentResponse>();

        var appointment = await _appointmentRepository
            .GetAll()
            .Include(appointment => appointment.Doctor)
            .Include(appointment => appointment.Patient)
            .Include(appointment => appointment.Type)
            .Include(appointment => appointment.Status)
            .FirstOrDefaultAsync(appointment => appointment.Id == request.AppointmentId, cancellationToken);

        if (appointment is null)
        {
            return result
                .WithError(new Error {Message = $"Appointment with id {request.AppointmentId} not found"})
                .WithStatusCode(StatusCodes.Status404NotFound);
        }

        var userCanAccessAppointment = _appointmentService.CanUserAccessAppointment(
            userId: request.UserId,
            role: request.RoleName,
            appointmentDoctorId: appointment.DoctorId,
            appointmentPatientId: appointment.PatientId
        );

        if (userCanAccessAppointment.IsError)
            return result
                .WithError(userCanAccessAppointment.Error)
                .WithStatusCode(StatusCodes.Status403Forbidden);

        if (!userCanAccessAppointment.IsError && userCanAccessAppointment.Value)
        {
            var appointmentResponse = _mapper.Map<AppointmentResponse>(appointment);
            return result.WithValue(appointmentResponse);
        }

        return result
            .WithError(new Error {Message = "Something went wrong"})
            .WithStatusCode(StatusCodes.Status500InternalServerError);
    }
}